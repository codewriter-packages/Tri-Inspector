using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriLabelWidthContextElement : VisualElement
    {
        public float LabelWidth { get; }

        public TriLabelWidthContextElement(float labelWidth, VisualElement content)
        {
            LabelWidth = labelWidth;

            Add(content);
        }
    }
}