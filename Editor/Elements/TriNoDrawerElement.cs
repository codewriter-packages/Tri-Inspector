using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    public class TriNoDrawerElement : TriElement
    {
        private readonly GUIContent _message;
        private readonly TriProperty _property;

        public TriNoDrawerElement(TriProperty property)
        {
            _property = property;
            _message = new GUIContent($"No drawer for {property.FieldType}");
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            return new TriAlignedLabel(property.DisplayName, new Label(_message.text)
            {
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            });
        }

        public override float GetHeight(float width)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position)
        {
            EditorGUI.LabelField(position, _property.DisplayNameContent, _message);
        }
    }
}