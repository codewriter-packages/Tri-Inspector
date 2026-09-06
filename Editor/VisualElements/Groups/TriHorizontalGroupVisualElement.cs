using System;
using TriInspector.Utilities;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements.Groups
{
    public class TriHorizontalGroupVisualElement : TriPropertyCollectionVisualElement
    {
        private readonly float[] _sizes;
        private int _childIndex;

        public TriHorizontalGroupVisualElement(float[] sizes = null)
        {
            _sizes = sizes ?? Array.Empty<float>();

            AddToClassList(TriStyles.HorizontalGroup);
        }

        protected override void AddPropertyChild(VisualElement child, TriProperty property)
        {
            var index = _childIndex++;

            var wrapper = new VisualElement();
            wrapper.AddToClassList(TriStyles.HorizontalGroupColumn);
            wrapper.AddToClassList(TriStyles.UnityInspectorElement);
            wrapper.AddToClassList(TriStyles.UnityInspectorMainContainer);
            wrapper.AddToClassList(TriStyles.TriInspectorElement);

            TriColumnSizes.Apply(wrapper, _sizes, index);

            if (index > 0)
            {
                wrapper.style.marginLeft = 2;
            }

            wrapper.Add(child);

            Add(wrapper);
        }
    }
}
