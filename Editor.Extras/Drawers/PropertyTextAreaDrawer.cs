using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(PropertyTextAreaDrawer), TriDrawerOrder.Decorator,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class PropertyTextAreaDrawer : TriAttributeDrawer<PropertyTextAreaAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var type = propertyDefinition.FieldType;
            if (type != typeof(string))
            {
                return "PropertyTextArea attribute can only be used on field";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new TextAreaElement(property);
        }

        private class TextAreaElement : TriElement
        {
            private readonly TriProperty _property;

            public TextAreaElement(TriProperty property)
            {
                _property = property;
            }

            public override float GetHeight(float width)
            {
                var text = _property.Value as string ?? "";
                return GUI.skin.textArea.CalcHeight(EditorGUIUtility.TrTempContent(text), width);
            }

            public override void OnGUI(Rect position)
            {
                var text = _property.Value as string ?? "";

                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);

                EditorGUI.TextArea(position, text);
            }
        }
    }
}