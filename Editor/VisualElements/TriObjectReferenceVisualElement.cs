using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriObjectReferenceVisualElement : VisualElement
    {
        public TriObjectReferenceVisualElement(TriValue<Object> propertyValue)
        {
            var allowSceneObjects = propertyValue.Property.PropertyTree.TargetIsPersistent == false;

            var field = new ObjectField
            {
                objectType = propertyValue.Property.FieldType,
                allowSceneObjects = allowSceneObjects,
                value = propertyValue.SmartValue,
                showMixedValue = propertyValue.Property.IsValueMixed,
            };

            field.AddToClassList(BaseField<Object>.alignedFieldUssClassName);

            field.RegisterValueChangedCallback(evt => propertyValue.SetValue(evt.newValue));

            field.AutoSyncLabelFromProperty(propertyValue.Property);
            field.AutoSyncValueFromProperty(propertyValue.Property);

            Add(field);
        }
    }
}