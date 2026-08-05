using System;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriInlineGenericVisualElement : VisualElement
    {
        [Serializable]
        public struct Props
        {
            public bool drawPrefixLabel;
            public float labelWidth;
        }

        public TriInlineGenericVisualElement(TriProperty property, Props props = default)
        {
            VisualElement content = new TriPropertyCollectionVisualElement(property.ValueType, property.ChildrenProperties);

            content = new TriLabelWidthContextVisualElement(props.labelWidth, content);

            if (props.drawPrefixLabel)
            {
                content.AddToClassList(TriStyles.UnityInspectorMainContainer);

                content = new TriAlignedLabelVisualElement(property, content);
            }

            Add(content);
        }
    }
}