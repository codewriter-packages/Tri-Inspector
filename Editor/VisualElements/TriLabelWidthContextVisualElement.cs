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

        public static bool ApplyWidthFromAncestorToPrefixLabel(VisualElement el)
        {
            if (el.FindAncestor<TriLabelWidthContextVisualElement>() is not { } labelContext)
            {
                return false;
            }

            if (labelContext.LabelWidth is not { } customLabelWidth)
            {
                return true;
            }

            if (el.Q<Label>(className: BaseField<object>.labelUssClassName) is not { } childLabel)
            {
                return false;
            }

            if (el.ClassListContains(BaseField<object>.alignedFieldUssClassName))
            {
                el.RemoveFromClassList(BaseField<object>.alignedFieldUssClassName);
                childLabel.style.width = childLabel.style.minWidth = customLabelWidth;
            }
            else
            {
                return true;
            }

            return false;
        }
    }
}