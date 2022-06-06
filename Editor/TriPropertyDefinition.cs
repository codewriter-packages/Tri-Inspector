using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TriInspector.Resolvers;
using TriInspector.Utilities;
using UnityEngine;

namespace TriInspector
{
    public class TriPropertyDefinition
    {
        private readonly ValueGetterDelegate _valueGetter;
        [CanBeNull] private readonly ValueSetterDelegate _valueSetter;

        private readonly List<string> _extensionErrors = new List<string>();

        private TriPropertyDefinition _arrayElementDefinitionBackingField;

        private IReadOnlyList<TriCustomDrawer> _drawersBackingField;
        private IReadOnlyList<TriValidator> _validatorsBackingField;
        private IReadOnlyList<TriPropertyHideProcessor> _hideProcessorsBackingField;
        private IReadOnlyList<TriPropertyDisableProcessor> _disableProcessorsBackingField;

        internal TriPropertyDefinition(int order, FieldInfo fi)
            : this(fi, order, fi.Name, fi.FieldType, MakeGetter(fi), MakeSetter(fi), false)
        {
        }

        internal TriPropertyDefinition(int order, PropertyInfo pi)
            : this(pi, order, pi.Name, pi.PropertyType, MakeGetter(pi), MakeSetter(pi), false)
        {
        }

        internal TriPropertyDefinition(int order, MethodInfo mi)
            : this(mi, order, mi.Name, typeof(MethodInfo), MakeGetter(mi), MakeSetter(mi), false)
        {
        }

        internal TriPropertyDefinition(
            MemberInfo memberInfo,
            int order,
            string fieldName,
            Type fieldType,
            ValueGetterDelegate valueGetter,
            ValueSetterDelegate valueSetter,
            bool isArrayElement)
        {
            MemberInfo = memberInfo;
            Name = fieldName;
            FieldType = fieldType;
            IsArrayElement = isArrayElement;

            _valueGetter = valueGetter;
            _valueSetter = valueSetter;

            Attributes = memberInfo?.GetCustomAttributes().ToList() ?? new List<Attribute>();
            Order = Attributes.TryGet(out PropertyOrderAttribute orderAttribute) ? orderAttribute.Order : order;
            IsReadOnly = _valueSetter == null || Attributes.TryGet(out ReadOnlyAttribute _);

            if (TriReflectionUtilities.IsArrayOrList(FieldType, out var elementType))
            {
                IsArray = true;
                ArrayElementType = elementType;
            }

            if (Attributes.TryGet(out LabelTextAttribute labelTextAttribute))
            {
                CustomLabel = ValueResolver.ResolveString(this, labelTextAttribute.Text);
            }

            if (Attributes.TryGet(out PropertyTooltipAttribute tooltipAttribute))
            {
                CustomTooltip = ValueResolver.ResolveString(this, tooltipAttribute.Tooltip);
            }
        }

        public MemberInfo MemberInfo { get; }

        public Type FieldType { get; }

        public string Name { get; }

        public int Order { get; }

        public IReadOnlyList<Attribute> Attributes { get; }

        public bool IsReadOnly { get; }

        public bool IsArrayElement { get; }
        public Type ArrayElementType { get; }

        public bool IsArray { get; }

        [CanBeNull] public ValueResolver<string> CustomLabel { get; }
        [CanBeNull] public ValueResolver<string> CustomTooltip { get; }

        public IReadOnlyList<TriPropertyHideProcessor> HideProcessors => PopulateHideProcessor();
        public IReadOnlyList<TriPropertyDisableProcessor> DisableProcessors => PopulateDisableProcessors();
        public IReadOnlyList<TriCustomDrawer> Drawers => PopulateDrawers();
        public IReadOnlyList<TriValidator> Validators => PopulateValidators();

        internal IReadOnlyList<string> ExtensionErrors
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

        public object GetValue(TriProperty property, int targetIndex)
        {
            return _valueGetter(property, targetIndex);
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

                    var elementMember = MemberInfo;
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

                    _arrayElementDefinitionBackingField = new TriPropertyDefinition(elementMember, 0, "Element",
                        ArrayElementType, elementGetter, elementSetter, true);
                }

                return _arrayElementDefinitionBackingField;
            }
        }

        private IReadOnlyList<TriPropertyHideProcessor> PopulateHideProcessor()
        {
            if (_hideProcessorsBackingField != null)
            {
                return _hideProcessorsBackingField;
            }

            return _hideProcessorsBackingField = TriDrawersUtilities
                .CreateHideProcessorsFor(Attributes)
                .Where(CanApplyExtensionOnSelf)
                .ToList();
        }

        private IReadOnlyList<TriPropertyDisableProcessor> PopulateDisableProcessors()
        {
            if (_disableProcessorsBackingField != null)
            {
                return _disableProcessorsBackingField;
            }

            return _disableProcessorsBackingField = TriDrawersUtilities
                .CreateDisableProcessorsFor(Attributes)
                .Where(CanApplyExtensionOnSelf)
                .ToList();
        }

        private IReadOnlyList<TriValidator> PopulateValidators()
        {
            if (_validatorsBackingField != null)
            {
                return _validatorsBackingField;
            }

            return _validatorsBackingField = Enumerable.Empty<TriValidator>()
                .Concat(TriDrawersUtilities.CreateValueValidatorsFor(FieldType))
                .Concat(TriDrawersUtilities.CreateAttributeValidatorsFor(Attributes))
                .Where(CanApplyExtensionOnSelf)
                .ToList();
        }

        private IReadOnlyList<TriCustomDrawer> PopulateDrawers()
        {
            if (_drawersBackingField != null)
            {
                return _drawersBackingField;
            }

            return _drawersBackingField = Enumerable.Empty<TriCustomDrawer>()
                .Concat(TriDrawersUtilities.CreateValueDrawersFor(FieldType))
                .Concat(TriDrawersUtilities.CreateAttributeDrawersFor(Attributes))
                .Concat(new[]
                {
                    new ValidatorsDrawer {Order = TriDrawerOrder.Validator,},
                })
                .Where(CanApplyExtensionOnSelf)
                .OrderBy(it => it.Order)
                .ToList();
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