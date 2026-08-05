using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriLabelWidthContextVisualElement : VisualElement
    {
        public float? LabelWidth { get; }

        public TriLabelWidthContextVisualElement(float? labelWidth, VisualElement child = null)
        {
            LabelWidth = labelWidth > 0 ? labelWidth : null;

            if (child != null)
            {
                Add(child);
            }
        }
    }
}