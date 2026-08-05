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

            field.AddToClassList(BaseField<TField>.alignedFieldUssClassName);

            field.SetValueWithoutNotify(toField(propertyValue.SmartValue));
            field.showMixedValue = property.IsValueMixed;

            field.RegisterValueChangedCallback(evt => propertyValue.SetValue(fromField(evt.newValue)));

            field.AutoSyncLabelFromProperty(property);
            field.AutoSyncValueFromProperty(property, () => toField(propertyValue.SmartValue));

            return field;
        }
    }
}