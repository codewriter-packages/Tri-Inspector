using TriInspector.VisualElements;
using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    internal class TriBuiltInPropertyElement : TriElement
    {
        private readonly TriProperty _property;
        private readonly PropertyHandlerProxy _propertyHandler;
        private readonly SerializedProperty _serializedProperty;

        public TriBuiltInPropertyElement(
            TriProperty property,
            SerializedProperty serializedProperty,
            PropertyHandlerProxy propertyHandler)
        {
            _property = property;
            _serializedProperty = serializedProperty;
            _propertyHandler = propertyHandler;
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            var label = _property.DisplayNameContent?.text ?? string.Empty;

            var field = new TriPropertyField(_serializedProperty, label);
            field.BindProperty(_serializedProperty);
            field.TrackPropertyValue(_serializedProperty, _ => _property.NotifyValueChanged());

            return field;
        }

        public override float GetHeight(float width)
        {
            return _propertyHandler.GetHeight(_serializedProperty, _property.DisplayNameContent, true);
        }

        public override void OnGUI(Rect position)
        {
            EditorGUI.BeginChangeCheck();

            if (_property.IsArrayElement &&
                _serializedProperty.propertyType == SerializedPropertyType.Generic &&
                _serializedProperty.hasVisibleChildren)
            {
                position.xMin += 12;
            }

            _propertyHandler.OnGUI(position, _serializedProperty, _property.DisplayNameContent, true);

            if (EditorGUI.EndChangeCheck())
            {
                _property.NotifyValueChanged();
            }
        }
    }
}