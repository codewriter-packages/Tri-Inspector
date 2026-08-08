using System;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    internal static class TriBuiltinFieldFactory
    {
        public static VisualElement Create<T>(TriValue<T> propertyValue, BaseField<T> field)
        {
            return Create(propertyValue, field, v => v, v => v);
        }

        public static VisualElement Create<TValue, TField>(
            TriValue<TValue> propertyValue,
            BaseField<TField> field,
            Func<TValue, TField> toField,
            Func<TField, TValue> fromField)
        {
            var property = propertyValue.Property;

            field.RegisterValueChangedCallback(evt => propertyValue.SetValue(fromField(evt.newValue)));
            field.AutoSyncValueFromProperty(property, () => toField(propertyValue.SmartValue));

            return new TriAlignedLabelVisualElement(property, field);
        }

        /// <summary>
        /// Builds a two-way bound aligned field for an attribute drawer that only has a
        /// <see cref="TriProperty"/> (no <see cref="TriValue{T}"/>), with custom value conversion.
        /// </summary>
        public static VisualElement CreateForProperty<TField>(
            TriProperty property,
            BaseField<TField> field,
            Func<TField> getValue,
            Action<TField> setValue)
        {
            field.RegisterValueChangedCallback(evt => setValue(evt.newValue));
            field.AutoSyncValueFromProperty(property, getValue);

            return new TriAlignedLabelVisualElement(property, field);
        }
    }
}