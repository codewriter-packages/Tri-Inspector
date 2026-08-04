using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriAlignedLabel : BaseField<object>
    {
        public TriAlignedLabel(string label, VisualElement content) : base(label, content)
        {
            AddToClassList(alignedFieldUssClassName);
        }
    }
}