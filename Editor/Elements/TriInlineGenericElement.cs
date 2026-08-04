using System;
using TriInspector.Utilities;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    internal class TriInlineGenericElement : TriPropertyCollectionBaseElement
    {
        private readonly Props _props;
        private readonly TriProperty _property;

        [Serializable]
        public struct Props
        {
            public bool drawPrefixLabel;
            public float labelWidth;
        }

        public TriInlineGenericElement(TriProperty property, Props props = default)
        {
            _property = property;
            _props = props;

            DeclareGroups(property.ValueType);

            foreach (var childProperty in property.ChildrenProperties)
            {
                AddProperty(childProperty);
            }
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            var content = CreateChildrenColumn(property);

            if (_props.labelWidth > 0)
            {
                content = new TriLabelWidthContextElement(_props.labelWidth, content);
            }

            if (!_props.drawPrefixLabel)
            {
                return content;
            }

            // Unity aligns property labels relative to element with this style
            content.AddToClassList("unity-inspector-main-container");

            return new TriAlignedLabel(_property.DisplayName, content);
        }

        public override void OnGUI(Rect position)
        {
            if (_props.drawPrefixLabel)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);
            }

            using (TriGuiHelper.PushLabelWidth(_props.labelWidth))
            {
                base.OnGUI(position);
            }
        }
    }
}