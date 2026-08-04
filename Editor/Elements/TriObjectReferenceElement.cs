using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TriInspector.Elements
{
    internal sealed class TriObjectReferenceElement : TriElement
    {
        private readonly TriValue<Object> _propertyValue;
        private readonly bool _allowSceneObjects;

        public TriObjectReferenceElement(TriValue<Object> propertyValue)
        {
            _propertyValue = propertyValue;
            _allowSceneObjects = propertyValue.Property.PropertyTree.TargetIsPersistent == false;
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            var field = new ObjectField(_propertyValue.Property.DisplayNameContent?.text)
            {
                objectType = _propertyValue.Property.FieldType,
                allowSceneObjects = _allowSceneObjects,
                value = _propertyValue.SmartValue,
                showMixedValue = _propertyValue.Property.IsValueMixed,
            };

            field.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            
            field.RegisterValueChangedCallback(evt => _propertyValue.SetValue(evt.newValue));

            field.schedule.Execute(() =>
            {
                field.showMixedValue = _propertyValue.Property.IsValueMixed;

                var current = _propertyValue.SmartValue;
                if (field.value != current)
                {
                    field.SetValueWithoutNotify(current);
                }
            }).Every(100);

            return field;
        }

        public override float GetHeight(float width)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position)
        {
            var value = _propertyValue.SmartValue;

            EditorGUI.BeginChangeCheck();

            value = EditorGUI.ObjectField(position, _propertyValue.Property.DisplayNameContent, value,
                _propertyValue.Property.FieldType, _allowSceneObjects);

            if (EditorGUI.EndChangeCheck())
            {
                _propertyValue.SetValue(value);
            }
        }
    }
}
