using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    internal class TriInlineGenericElement : TriPropertyCollectionBaseElement
    {
        private readonly bool _drawPrefixLabel;
        private readonly float _labelWidth;
        private readonly TriProperty _property;

        public TriInlineGenericElement(TriProperty property,
            bool drawPrefixLabel = false, float labelWidth = 0f)
        {
            _property = property;
            _drawPrefixLabel = drawPrefixLabel;
            _labelWidth = labelWidth;

            DeclareGroups(property.ValueType);

            foreach (var childProperty in property.ChildrenProperties)
            {
                AddProperty(childProperty);
            }
        }

        public override void OnGUI(Rect position)
        {
            if (_drawPrefixLabel)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);
            }

            TriGuiHelper.PushLabelWidth(_labelWidth);
            base.OnGUI(position);
            TriGuiHelper.PopLabelWidth();
        }
    }
}