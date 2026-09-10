using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using TriInspector.Resolvers;
using TriInspector.Utilities;
using UnityEngine;
using Attribute = System.Attribute;

namespace TriInspector
{
    public class TriPropertyDefinition
    {
        private readonly ValueGetterDelegate _valueGetter;
        [CanBeNull] private readonly ValueSetterDelegate _valueSetter;

        private readonly List<string> _extensionErrors = new List<string>();
        private readonly MemberInfo _memberInfo;
        private readonly Attribute[] _attributes;
        private readonly bool _skipNullValuesFix;

        private List<Attribute> _attributesDynamicNullable;
        private TriPropertyDefinition _arrayElementDefinitionBackingField;

        private TriArray<TriCustomDrawer>? _drawersBackingField;
        private TriArray<TriValidator>? _validatorsBackingField;
        private TriArray<TriPropertyHideProcessor>? _hideProcessorsBackingField;
        private TriArray<TriPropertyDisableProcessor>? _disableProcessorsBackingField;

        public static TriPropertyDefinition CreateForFieldInfo(int order, FieldInfo fi,
            TriPropertyOrigin origin = TriPropertyOrigin.Unknown)
        {
            return CreateForMemberInfo(fi, order, fi.Name, fi.FieldType, MakeGetter(fi), MakeSetter(fi), origin);
        }

        public static TriPropertyDefinition CreateForPropertyInfo(int order, PropertyInfo pi,
            TriPropertyOrigin origin = TriPropertyOrigin.Unknown)
        {
            return CreateForMemberInfo(pi, order, pi.Name, pi.PropertyType, MakeGetter(pi), MakeSetter(pi), origin);
        }

        public static TriPropertyDefinition CreateForMethodInfo(int order, MethodInfo mi,
            TriPropertyOrigin origin = TriPropertyOrigin.Unknown)
        {
            return CreateForMemberInfo(mi, order, mi.Name, typeof(MethodInfo), MakeGetter(mi), MakeSetter(mi), origin);
        }

        private static TriPropertyDefinition CreateForMemberInfo(
            MemberInfo memberInfo, int order, string propertyName, Type propertyType,
            ValueGetterDelegate valueGetter, ValueSetterDelegate valueSetter,
            TriPropertyOrigin origin = TriPropertyOrigin.Unknown)
        {
            var attributes = TriReflectionUtilities.GetCustomNonSerializationAttributes(memberInfo);
            var ownerType = memberInfo?.DeclaringType ?? typeof(object);

            return new TriPropertyDefinition(
                memberInfo, ownerType, order, propertyName, propertyType, valueGetter, valueSetter, attributes, false,
                origin);
        }

        internal static TriPropertyDefinition CreateForGetterSetter(
            int order, string name, Type fieldType,
            ValueGetterDelegate valueGetter, ValueSetterDelegate valueSetter)
        {
            return new TriPropertyDefinition(
                null, null, order, name, fieldType, valueGetter, valueSetter, null, false);
        }

        internal TriPropertyDefinition(
            MemberInfo memberInfo,
            Type ownerType,
            int order,
            string fieldName,
            Type fieldType,
            ValueGetterDelegate valueGetter,
            ValueSetterDelegate valueSetter,
            Attribute[] attributes,
            bool isArrayElement,
            TriPropertyOrigin origin = TriPropertyOrigin.Unknown)
        {
            OwnerType = ownerType;
            Name = fieldName;
            FieldType = fieldType;
            IsArrayElement = isArrayElement;
            Origin = origin;

            _attributes = attributes ?? Array.Empty<Attribute>();
            _memberInfo = memberInfo;
            _valueGetter = valueGetter;
            _valueSetter = valueSetter;

            _skipNullValuesFix = memberInfo != null && memberInfo.GetCustomAttribute<SerializeReference>() != null;

            Order = order;
            IsReadOnly = _valueSetter == null || Attributes.TryGet(out ReadOnlyAttribute _);

            if (TriReflectionUtilities.IsArrayOrListOrDictionary(FieldType, out var elementType, out var isDictionary))
            {
                IsArray = true;

                if (isDictionary)
                {
                    IsDictionary = true;
                    ArrayElementType = typeof(TriDictionaryEntry<,>).MakeGenericType(fieldType.GetGenericArguments());
                    FieldType = typeof(List<>).MakeGenericType(ArrayElementType);
                    GetEditableAttributes().Add(new HideReferencePickerAttribute());
                    (_valueGetter, _valueSetter) = ConfigureDictionaryAsListSerialization(
                        ArrayElementType, valueGetter, valueSetter);
                }
                else
                {
                    ArrayElementType = elementType;
                }
            }

            if (Attributes.TryGet(out LabelTextAttribute labelTextAttribute))
            {
                CustomLabel = ValueResolver.ResolveString(this, labelTextAttribute.Text);
            }

            if (Attributes.TryGet(out PropertyTooltipAttribute tooltipAttribute))
            {
                CustomTooltip = ValueResolver.ResolveString(this, tooltipAttribute.Tooltip);
            }
            else if (Attributes.TryGet(out TooltipAttribute unityTooltipAttribute))
            {
                CustomTooltip = new ConstantValueResolver<string>(unityTooltipAttribute.tooltip);
            }
        }

        public Type OwnerType { get; }

        public Type FieldType { get; }

        public string Name { get; }

        public int Order { get; internal set; }

        public TriPropertyOrigin Origin { get; }

        public TriArray<Attribute> Attributes => _attributesDynamicNullable != null
            ? _attributesDynamicNullable
            : _attributes;

        public bool IsReadOnly { get; }

        public bool IsArrayElement { get; }
        public Type ArrayElementType { get; }

        public bool IsArray { get; }
        public bool IsDictionary { get; }

        [CanBeNull] public ValueResolver<string> CustomLabel { get; }
        [CanBeNull] public ValueResolver<string> CustomTooltip { get; }

        public TriArray<TriPropertyHideProcessor> HideProcessors => PopulateHideProcessor();
        public TriArray<TriPropertyDisableProcessor> DisableProcessors => PopulateDisableProcessors();
        public TriArray<TriCustomDrawer> Drawers => PopulateDrawers();
        public TriArray<TriValidator> Validators => PopulateValidators();

        internal TriArray<string> ExtensionErrors
        {
            get
            {
                PopulateHideProcessor();
                PopulateDisableProcessors();
                PopulateDrawers();
                PopulateValidators();
                return _extensionErrors;
            }
        }

        public List<Attribute> GetEditableAttributes()
        {
            _attributesDynamicNullable ??= new List<Attribute>(_attributes);
            return _attributesDynamicNullable;
        }

        public bool TryGetMemberInfo(out MemberInfo memberInfo)
        {
            memberInfo = _memberInfo;
            return memberInfo != null;
        }

        public object GetValue(TriProperty property, int targetIndex)
        {
            var value = _valueGetter(property, targetIndex);

            if (value == null && !_skipNullValuesFix)
            {
                value = TriUnitySerializationUtilities.PopulateUnityDefaultValueForType(FieldType);

                if (value != null)
                {
                    _valueSetter?.Invoke(property, targetIndex, value);
                }
            }

            return value;
        }

        public bool SetValue(TriProperty property, object value, int targetIndex, out object parentValue)
        {
            if (IsReadOnly)
            {
                Debug.LogError("Cannot set value for readonly property");
                parentValue = default;
                return false;
            }

            parentValue = _valueSetter?.Invoke(property, targetIndex, value);
            return true;
        }

        public TriPropertyDefinition ArrayElementDefinition
        {
            get
            {
                if (_arrayElementDefinitionBackingField == null)
                {
                    if (!IsArray)
                    {
                        throw new InvalidOperationException(
                            $"Cannot get array element definition for non array property: {FieldType}");
                    }

                    var elementGetter = new ValueGetterDelegate((self, targetIndex) =>
                    {
                        var parentValue = (IList) self.Parent.GetValue(targetIndex);
                        return parentValue[self.IndexInArray];
                    });
                    var elementSetter = new ValueSetterDelegate((self, targetIndex, value) =>
                    {
                        var parentValue = (IList) self.Parent.GetValue(targetIndex);
                        parentValue[self.IndexInArray] = value;
                        return parentValue;
                    });

                    _arrayElementDefinitionBackingField = new TriPropertyDefinition(_memberInfo, OwnerType, 0,
                        "Element", ArrayElementType, elementGetter, elementSetter, _attributes, true, Origin)
                    {
                        _attributesDynamicNullable = _attributesDynamicNullable,
                    };
                }

                return _arrayElementDefinitionBackingField;
            }
        }

        private TriArray<TriPropertyHideProcessor> PopulateHideProcessor()
        {
            if (_hideProcessorsBackingField != null)
            {
                return _hideProcessorsBackingField.Value;
            }

            List<TriPropertyHideProcessor> processors = null;
            TriDrawersUtilities.CreateHideProcessorsFor(ref processors, FieldType, Attributes);
            RemoveNonApplicableOnSelf(processors);
            return (_hideProcessorsBackingField = processors).Value;
        }

        private TriArray<TriPropertyDisableProcessor> PopulateDisableProcessors()
        {
            if (_disableProcessorsBackingField != null)
            {
                return _disableProcessorsBackingField.Value;
            }

            List<TriPropertyDisableProcessor> processors = null;
            TriDrawersUtilities.CreateDisableProcessorsFor(ref processors, FieldType, Attributes);
            RemoveNonApplicableOnSelf(processors);
            return (_disableProcessorsBackingField = processors).Value;
        }

        private TriArray<TriValidator> PopulateValidators()
        {
            if (_validatorsBackingField != null)
            {
                return _validatorsBackingField.Value;
            }

            List<TriValidator> validators = null;
            TriDrawersUtilities.CreateValueValidatorsFor(ref validators, FieldType);
            TriDrawersUtilities.CreateAttributeValidatorsFor(ref validators, FieldType, Attributes);
            RemoveNonApplicableOnSelf(validators);
            return (_validatorsBackingField = validators).Value;
        }

        private TriArray<TriCustomDrawer> PopulateDrawers()
        {
            if (_drawersBackingField != null)
            {
                return _drawersBackingField.Value;
            }

            var drawers = new List<TriCustomDrawer>
            {
                new ValidatorsDrawer {Order = TriDrawerOrder.Validator,},
            };

            TriDrawersUtilities.CreateValueDrawersFor(ref drawers, FieldType);
            TriDrawersUtilities.CreateAttributeDrawersFor(ref drawers, FieldType, Attributes);

            if (TriReflectionUtilities.GetCustomNonSerializationAttributes(FieldType) is { } typeAttributes)
            {
                TriDrawersUtilities.CreateAttributeDrawersFor(ref drawers, FieldType, typeAttributes);
            }

            RemoveNonApplicableOnSelf(drawers);
            drawers.Sort(static (a, b) => a.Order.CompareTo(b.Order));
            return (_drawersBackingField = drawers).Value;
        }

        private static ValueGetterDelegate MakeGetter(FieldInfo fi)
        {
            return (self, targetIndex) =>
            {
                var parentValue = self.Parent.GetValue(targetIndex);
                return fi.GetValue(parentValue);
            };
        }

        private static ValueSetterDelegate MakeSetter(FieldInfo fi)
        {
            return (self, targetIndex, value) =>
            {
                var parentValue = self.Parent.GetValue(targetIndex);
                fi.SetValue(parentValue, value);
                return parentValue;
            };
        }

        private static ValueGetterDelegate MakeGetter(PropertyInfo pi)
        {
            var method = pi.GetMethod;
            return (self, targetIndex) =>
            {
                var parentValue = self.Parent.GetValue(targetIndex);
                return method.Invoke(parentValue, null);
            };
        }

        private static ValueSetterDelegate MakeSetter(PropertyInfo pi)
        {
            var method = pi.SetMethod;
            if (method == null)
            {
                return null;
            }

            return (self, targetIndex, value) =>
            {
                var parentValue = self.Parent.GetValue(targetIndex);
                method.Invoke(parentValue, new[] {value,});
                return parentValue;
            };
        }

        private static ValueGetterDelegate MakeGetter(MethodInfo mi)
        {
            return (self, targetIndex) => mi;
        }

        private static ValueSetterDelegate MakeSetter(MethodInfo mi)
        {
            return (self, targetIndex, value) =>
            {
                var parentValue = self.Parent.GetValue(targetIndex);
                return parentValue;
            };
        }

        private static (ValueGetterDelegate, ValueSetterDelegate) ConfigureDictionaryAsListSerialization(
            Type arrayElementType,
            ValueGetterDelegate valueGetter, ValueSetterDelegate valueSetter)
        {
            var makeList = arrayElementType.GetMethod(nameof(TriDictionaryEntry<int, int>.MakeList));
            var makeDict = arrayElementType.GetMethod(nameof(TriDictionaryEntry<int, int>.MakeDict));
#if UNITY_6000_6_OR_NEWER
            var makeListFromSerializedProperty = arrayElementType.GetMethod(
                nameof(TriDictionaryEntry<int, int>.MakeListFromSerializedProperty));
            var writeToSerializedProperty = arrayElementType.GetMethod(
                nameof(TriDictionaryEntry<int, int>.WriteToSerializedProperty));
#endif

            object GetDictionaryAsList(TriProperty self, int targetIndex)
            {
                var duplicateEntryIndices = self.DictionaryDuplicateEntryIndicesBuffer;
                var nullKeyEntryIndices = self.DictionaryNullKeyEntryIndicesBuffer;

                if (self.PropertyTree.TargetsCount != 1)
                {
                    duplicateEntryIndices.Clear();
                    nullKeyEntryIndices.Clear();
                    return makeList!.Invoke(null, new object[] {null,});
                }

#if UNITY_6000_6_OR_NEWER
                if (self.TryGetSerializedProperty(out var serializedProperty))
                {
                    var listFromSerialized =
                        makeListFromSerializedProperty!.Invoke(null, new object[] {serializedProperty,});

                    var ignored = serializedProperty.GetDictionaryIgnoredEntries();

                    duplicateEntryIndices.Clear();
                    duplicateEntryIndices.AddRange(ignored.duplicateEntryIndices ?? Array.Empty<int>());

                    nullKeyEntryIndices.Clear();
                    nullKeyEntryIndices.AddRange(ignored.nullKeyEntryIndices ?? Array.Empty<int>());

                    return listFromSerialized;
                }
#endif

                if (self.DictionaryListCache is { } cachedList &&
                    (duplicateEntryIndices.Count > 0 || nullKeyEntryIndices.Count > 0))
                {
                    return cachedList;
                }

                var dict = (IDictionary) valueGetter(self, targetIndex);
                var list = makeList!.Invoke(null, new object[] {dict,});
                self.DictionaryListCache = list;
                return list;
            }

            object SetListAsDictionary(TriProperty self, int targetIndex, object value)
            {
                if (self.PropertyTree.TargetsCount != 1)
                {
                    return self.Parent?.GetValue(targetIndex);
                }

#if UNITY_6000_6_OR_NEWER
                if (self.TryGetSerializedProperty(out var serializedProperty))
                {
                    writeToSerializedProperty!.Invoke(null, new object[] {value, serializedProperty,});
                    return self.Parent?.GetValue(targetIndex);
                }
#endif

                var dict = makeDict!.Invoke(null,
                    new object[]
                    {
                        value,
                        self.DictionaryDuplicateEntryIndicesBuffer,
                        self.DictionaryNullKeyEntryIndicesBuffer,
                    });
                self.DictionaryListCache = value;
                return valueSetter(self, targetIndex, dict);
            }

            return (GetDictionaryAsList, valueSetter != null ? SetListAsDictionary : null);
        }

        private void RemoveNonApplicableOnSelf<T>(List<T> list) where T : TriPropertyExtension
        {
            if (list == null)
            {
                return;
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (!CanApplyExtensionOnSelf(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        private bool CanApplyExtensionOnSelf(TriPropertyExtension propertyExtension)
        {
            if (propertyExtension.ApplyOnArrayElement.HasValue)
            {
                if (IsArrayElement && !propertyExtension.ApplyOnArrayElement.Value ||
                    IsArray && propertyExtension.ApplyOnArrayElement.Value)
                {
                    return false;
                }
            }

            var result = propertyExtension.Initialize(this);
            if (result.IsError)
            {
                _extensionErrors.Add(result.ErrorMessage);
            }

            return result.ShouldApply;
        }

        public delegate object ValueGetterDelegate(TriProperty self, int targetIndex);

        public delegate object ValueSetterDelegate(TriProperty self, int targetIndex, object value);
    }
}