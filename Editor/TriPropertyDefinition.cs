using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TriInspector.Utilities;
using UnityEngine;

namespace TriInspector
{
    public class TriPropertyDefinition
    {
        private readonly Func<TriProperty, object, object> _valueGetter;
        [CanBeNull] private readonly Action<TriProperty, object, object> _valueSetter;

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

        private TriPropertyDefinition(
            MemberInfo memberInfo,
            int order,
            string fieldName,
            Type fieldType,
            Func<TriProperty, object, object> valueGetter,
            Action<TriProperty, object, object> valueSetter,
            bool isArrayElement)
        {
            MemberInfo = memberInfo;
            Name = fieldName;
            FieldType = fieldType;
            IsArrayElement = isArrayElement;

            _valueGetter = valueGetter;
            _valueSetter = valueSetter;

            Attributes = memberInfo.GetCustomAttributes().ToList();
            Order = Attributes.TryGet(out PropertyOrderAttribute orderAttribute) ? orderAttribute.Order : order;
            IsReadOnly = _valueSetter == null || Attributes.TryGet(out ReadOnlyAttribute _);

            if (TriReflectionUtilities.IsArrayOrList(FieldType, out var elementType))
            {
                IsArray = true;
                ArrayElementType = elementType;
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

        public IReadOnlyList<TriPropertyHideProcessor> HideProcessors
        {
            get
            {
                if (_hideProcessorsBackingField == null)
                {
                    _hideProcessorsBackingField =
                        TriDrawersUtilities.CreateHideProcessorsFor(Attributes)
                            .Where(it => CanApplyOn(this, applyOnArrayElement: false))
                            .ToList();

                    foreach (var processor in _hideProcessorsBackingField)
                    {
                        processor.Initialize(this);
                    }
                }

                return _hideProcessorsBackingField;
            }
        }

        public IReadOnlyList<TriPropertyDisableProcessor> DisableProcessors
        {
            get
            {
                if (_disableProcessorsBackingField == null)
                {
                    _disableProcessorsBackingField =
                        TriDrawersUtilities.CreateDisableProcessorsFor(Attributes)
                            .Where(it => CanApplyOn(this, applyOnArrayElement: false))
                            .ToList();

                    foreach (var processor in _disableProcessorsBackingField)
                    {
                        processor.Initialize(this);
                    }
                }

                return _disableProcessorsBackingField;
            }
        }

        public IReadOnlyList<TriCustomDrawer> Drawers
        {
            get
            {
                if (_drawersBackingField == null)
                {
                    _drawersBackingField = Enumerable.Empty<TriCustomDrawer>()
                        .Concat(TriDrawersUtilities.CreateValueDrawersFor(FieldType))
                        .Concat(TriDrawersUtilities.CreateAttributeDrawersFor(Attributes))
                        .Where(it => CanApplyOn(this, it.ApplyOnArrayElement))
                        .OrderBy(it => it.Order)
                        .ToList();

                    foreach (var drawer in _drawersBackingField)
                    {
                        drawer.Initialize(this);
                    }
                }

                return _drawersBackingField;
            }
        }

        public IReadOnlyList<TriValidator> Validators
        {
            get
            {
                if (_validatorsBackingField == null)
                {
                    _validatorsBackingField = Enumerable.Empty<TriValidator>()
                        .Concat(TriDrawersUtilities.CreateValueValidatorsFor(FieldType))
                        .Concat(TriDrawersUtilities.CreateAttributeValidatorsFor(Attributes))
                        .Where(it => CanApplyOn(this, it.ApplyOnArrayElement))
                        .ToList();

                    foreach (var validator in _validatorsBackingField)
                    {
                        validator.Initialize(this);
                    }
                }

                return _validatorsBackingField;
            }
        }

        public object GetValue(TriProperty property, int targetIndex)
        {
            var parentValue = property.Parent.GetValue(targetIndex);
            return _valueGetter(property, parentValue);
        }

        public bool SetValue(TriProperty property, object value, int targetIndex, out object parentValue)
        {
            if (IsReadOnly)
            {
                Debug.LogError("Cannot set value for readonly property");
                parentValue = default;
                return false;
            }

            parentValue = property.Parent.GetValue(targetIndex);
            _valueSetter?.Invoke(property, parentValue, value);
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
                    var elementGetter = new Func<TriProperty, object, object>((self, obj) =>
                    {
                        var list = (IList) obj;
                        return list[self.IndexInArray];
                    });
                    var elementSetter = new Action<TriProperty, object, object>((self, obj, value) =>
                    {
                        var list = (IList) obj;
                        list[self.IndexInArray] = value;
                    });

                    _arrayElementDefinitionBackingField = new TriPropertyDefinition(elementMember, 0, "Element",
                        ArrayElementType, elementGetter, elementSetter, true);
                }

                return _arrayElementDefinitionBackingField;
            }
        }

        private static Func<TriProperty, object, object> MakeGetter(FieldInfo fi)
        {
            return (self, obj) => fi.GetValue(obj);
        }

        private static Action<TriProperty, object, object> MakeSetter(FieldInfo fi)
        {
            return (self, obj, value) => fi.SetValue(obj, value);
        }

        private static Func<TriProperty, object, object> MakeGetter(PropertyInfo pi)
        {
            var method = pi.GetMethod;
            return (self, obj) => method.Invoke(obj, null);
        }

        private static Action<TriProperty, object, object> MakeSetter(PropertyInfo pi)
        {
            var method = pi.SetMethod;
            if (method == null)
            {
                return null;
            }

            return (self, obj, value) => method.Invoke(obj, new[] {value,});
        }

        private static Func<TriProperty, object, object> MakeGetter(MethodInfo mi)
        {
            return (self, obj) => mi;
        }

        private static Action<TriProperty, object, object> MakeSetter(MethodInfo mi)
        {
            return (self, obj, value) => { };
        }

        private static bool CanApplyOn(TriPropertyDefinition definition, bool applyOnArrayElement)
        {
            if (definition.IsArrayElement && !applyOnArrayElement ||
                definition.IsArray && applyOnArrayElement)
            {
                return false;
            }

            return true;
        }
    }
}