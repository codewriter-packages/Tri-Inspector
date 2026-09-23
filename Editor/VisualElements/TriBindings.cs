using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    internal static class TriBindings
    {
        public static void BindTri<TValue>(
            this BaseField<TValue> field,
            TriValue<TValue> propertyValue,
            bool hideLabel = false
        )
        {
            field.BindTri(propertyValue.Property, static v => v, static v => v, hideLabel);
        }

        public static void BindTri<TValue>(
            this BaseField<TValue> field,
            TriProperty property,
            bool hideLabel = false
        )
        {
            field.BindTri(property, static v => v, static v => v, hideLabel);
        }

        public static void BindTri<TRawValue, TFieldValue>(
            this BaseField<TFieldValue> field,
            TriValue<TRawValue> propertyValue,
            Func<TRawValue, TFieldValue> toField,
            Func<TFieldValue, TRawValue> fromField,
            bool hideLabel = false)
        {
            BindTri(field, propertyValue.Property, toField, fromField, hideLabel);
        }

        public static void BindTri<TRawValue, TFieldValue>(
            this BaseField<TFieldValue> field,
            TriProperty property,
            Func<TRawValue, TFieldValue> toField,
            Func<TFieldValue, TRawValue> fromField,
            bool hideLabel = false)
        {
            // Prefer Unity's native SerializedProperty binding when the field's value type matches the
            // property's raw type: it handles two-way sync, undo and mixed-value display. TriProperty
            // still reacts to native writes because TriPropertyVisualElement tracks the serialized
            // property (TrackPropertyValue -> RefreshValue), which fires ValueChanged and re-runs
            // validation. When a conversion is involved (e.g. an ObjectField backed by a string path, or
            // a float Slider backed by an int), or there is no serialized property (a non-serialized C#
            // member), fall back to the manual write-back + poll instead.
            if (typeof(TRawValue) == typeof(TFieldValue) &&
                property.TryGetSerializedProperty(out var serializedProperty))
            {
                field.BindProperty(serializedProperty);
            }
            else
            {
                field.RegisterValueChangedCallback(evt => property.SetValue(fromField(evt.newValue)));
                field.AutoSyncValueFromProperty(property, v => toField((TRawValue) v));
            }

            if (hideLabel)
            {
                field.label = null;
            }
            else
            {
                field.AutoSyncLabelFromProperty(property);
                TriLabelWidthContextVisualElement.SetupAlignedLabel(field);
            }
        }
    }
}