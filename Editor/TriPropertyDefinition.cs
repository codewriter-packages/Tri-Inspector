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
    internal class TriPropertyDefinition
    {
        private readonly Func<TriProperty, object, object> _valueGetter;
        [CanBeNull] private readonly Action<TriProperty, object, object> _valueSetter;

        private TriPropertyDefinition _arrayElementDefinitionBackingField;

        private IReadOnlyList<TriCustomDrawer> _drawersBackingField;
        private IReadOnlyList<TriPropertyHideProcessor> _hideProcessorsBackingField;
        private IReadOnlyList<TriPropertyDisableProcessor> _disableProcessorsBackingField;

        internal TriPropertyDefinition(int order, FieldInfo fi)
            : this(order, fi.Name, fi.FieldType, MakeGetter(fi), MakeSetter(fi), fi.GetCustomAttributes(), false)
        {
        }

        internal TriPropertyDefinition(int order, PropertyInfo pi)
            : this(order, pi.Name, pi.PropertyType, MakeGetter(pi), MakeSetter(pi), pi.GetCustomAttributes(), false)
        {
        }

        private TriPropertyDefinition(
            int order,
            string fieldName,
            Type fieldType,
            Func<TriProperty, object, object> valueGetter,
            Action<TriProperty, object, object> valueSetter,
            IEnumerable<Attribute> fieldAttributes,
            bool isArrayElement)
        {
            Name = fieldName;
            FieldType = fieldType;
            IsArrayElement = isArrayElement;

            _valueGetter = valueGetter;
            _valueSetter = valueSetter;

            Attributes = fieldAttributes.ToList();
            Order = Attributes.TryGet(out PropertyOrderAttribute orderAttribute) ? orderAttribute.Order : order;
            IsReadOnly = _valueSetter == null || Attributes.TryGet(out ReadOnlyAttribute _);

            if (TriReflectionUtilities.IsArrayOrList(FieldType, out var elementType))
            {
                IsArray = true;
                ArrayElementType = elementType;
            }
        }

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
                    _hideProcessorsBackingField = (
                        from attribute in Attributes
                        from processor in TriDrawersUtilities.AllHideProcessors
                        where TriDrawersUtilities.IsHideProcessorFor(processor.ProcessorType, attribute)
                        select CreateHideProcessor(processor, attribute)
                    ).ToList();

                    static TriPropertyHideProcessor CreateHideProcessor(RegisterTriPropertyHideProcessor processor,
                        Attribute attribute)
                    {
                        var instance = (TriPropertyHideProcessor) Activator.CreateInstance(processor.ProcessorType);
                        instance.RawAttribute = attribute;
                        return instance;
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
                    _disableProcessorsBackingField = (
                        from attribute in Attributes
                        from processor in TriDrawersUtilities.AllDisableProcessors
                        where TriDrawersUtilities.IsDisableProcessorFor(processor.ProcessorType, attribute)
                        select CreateDisableProcessor(processor, attribute)
                    ).ToList();

                    static TriPropertyDisableProcessor CreateDisableProcessor(
                        RegisterTriPropertyDisableProcessor processor,
                        Attribute attribute)
                    {
                        var instance = (TriPropertyDisableProcessor) Activator.CreateInstance(processor.ProcessorType);
                        instance.RawAttribute = attribute;
                        return instance;
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
                    var valueDrawers =
                        from drawer in TriDrawersUtilities.AllValueDrawerTypes
                        where TriDrawersUtilities.IsValueDrawerFor(drawer.DrawerType, FieldType)
                        select CreateValueDrawer(drawer);

                    var attributeDrawers =
                        from attribute in Attributes
                        from drawer in TriDrawersUtilities.AllAttributeDrawerTypes
                        where TriDrawersUtilities.IsAttributeDrawerFor(drawer.DrawerType, attribute)
                        select CreateAttributeDrawer(drawer, attribute);

                    _drawersBackingField = Enumerable.Empty<TriCustomDrawer>()
                        .Concat(valueDrawers)
                        .Concat(attributeDrawers)
                        .OrderBy(it => it.Order)
                        .ToList();

                    static TriValueDrawer CreateValueDrawer(RegisterTriValueDrawerAttribute drawer)
                    {
                        var instance = (TriValueDrawer) Activator.CreateInstance(drawer.DrawerType);
                        instance.Target = drawer.Target;
                        instance.Order = drawer.Order;
                        return instance;
                    }

                    static TriAttributeDrawer CreateAttributeDrawer(RegisterTriAttributeDrawerAttribute drawer,
                        Attribute attribute)
                    {
                        var instance = (TriAttributeDrawer) Activator.CreateInstance(drawer.DrawerType);
                        instance.Target = drawer.Target;
                        instance.Order = drawer.Order;
                        instance.RawAttribute = attribute;
                        return instance;
                    }
                }

                return _drawersBackingField;
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
                    var elementAttributes = Attributes;

                    _arrayElementDefinitionBackingField = new TriPropertyDefinition(0, "Element", ArrayElementType,
                        elementGetter, elementSetter, elementAttributes, true);
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
    }
}