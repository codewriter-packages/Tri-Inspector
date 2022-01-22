using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(StringDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class StringDrawer : TriValueDrawer<string>
    {
        public override TriElement CreateElement(TriValue<string> value, TriElement next)
        {
            if (value.Property.TryGetSerializedProperty(out var serializedProperty))
            {
                return new StringSerializedPropertyDrawerElement(serializedProperty);
            }

            return new StringDrawerElement(value);
        }

        private class StringDrawerElement : TriElement
        {
            private TriValue<string> _propertyValue;

            public StringDrawerElement(TriValue<string> propertyValue)
            {
                _propertyValue = propertyValue;
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                var value = _propertyValue.Value;

                EditorGUI.BeginChangeCheck();

                value = EditorGUI.TextField(position, _propertyValue.Property.DisplayNameContent, value);

                if (EditorGUI.EndChangeCheck())
                {
                    _propertyValue.Value = value;
                }
            }
        }

        private class StringSerializedPropertyDrawerElement : TriElement
        {
            private readonly SerializedProperty _serializedProperty;

            public StringSerializedPropertyDrawerElement(SerializedProperty serializedProperty)
            {
                _serializedProperty = serializedProperty;
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                EditorGUI.PropertyField(position, _serializedProperty);
            }
        }
    }
}