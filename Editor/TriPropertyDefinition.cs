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
        private readonly Func<object, object> _valueGetter;
        [CanBeNull] private readonly Action<object, object> _valueSetter;

        private IReadOnlyList<TriCustomDrawer> _drawersBackingField;

        internal TriPropertyDefinition(int order, FieldInfo fi)
            : this(order, fi.Name, fi.FieldType, fi.GetValue, fi.SetValue, fi.GetCustomAttributes(), false)
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
            Func<object, object> valueGetter,
            Action<object, object> valueSetter,
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

                    static TriValueDrawer CreateValueDrawer(RegisterTriDrawerAttribute drawer)
                    {
                        var instance = (TriValueDrawer) Activator.CreateInstance(drawer.DrawerType);
                        instance.ApplyOnArrayElement = drawer.ApplyOnArrayElement;
                        instance.Order = drawer.Order;
                        return instance;
                    }

                    static TriAttributeDrawer CreateAttributeDrawer(RegisterTriDrawerAttribute drawer,
                        Attribute attribute)
                    {
                        var instance = (TriAttributeDrawer) Activator.CreateInstance(drawer.DrawerType);
                        instance.ApplyOnArrayElement = drawer.ApplyOnArrayElement;
                        instance.Order = drawer.Order;
                        instance.RawAttribute = attribute;
                        return instance;
                    }
                }

                return _drawersBackingField;
            }
        }

        public object GetValue(object obj)
        {
            return _valueGetter(obj);
        }

        public void SetValue(object obj, object value)
        {
            if (IsReadOnly)
            {
                Debug.LogError("Cannot set value for readonly property");
                return;
            }

            _valueSetter?.Invoke(obj, value);
        }

        public TriPropertyDefinition GetArrayElementDefinition(int index)
        {
            if (!IsArray)
            {
                throw new InvalidOperationException(
                    $"Cannot get array element definition for non array property: {FieldType}");
            }

            var elementName = $"Element {index}";
            var elementGetter = new Func<object, object>(obj => ((IList) obj)[index]);
            var elementSetter = new Action<object, object>((obj, value) => ((IList) obj)[index] = value);
            var elementOrder = index;
            var elementAttributes = Attributes;

            var definition = new TriPropertyDefinition(elementOrder, elementName, ArrayElementType,
                elementGetter, elementSetter, elementAttributes, true);

            return definition;
        }

        private static Func<object, object> MakeGetter(PropertyInfo pi)
        {
            var method = pi.GetMethod;
            return obj => method.Invoke(obj, null);
        }

        private static Action<object, object> MakeSetter(PropertyInfo pi)
        {
            var method = pi.SetMethod;
            if (method == null)
            {
                return null;
            }

            return (obj, value) => method.Invoke(obj, new[] {value,});
        }
    }
}