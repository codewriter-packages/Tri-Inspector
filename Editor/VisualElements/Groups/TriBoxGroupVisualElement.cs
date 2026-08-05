using UnityEngine.UIElements;

namespace TriInspector.VisualElements.Groups
{
    public class TriBoxGroupVisualElement : TriBoxGroupBaseVisualElement
    {
        public TriBoxGroupVisualElement(bool hideIfChildrenInvisible)
            : base(null, hideIfChildrenInvisible)
        {
            var content = new VisualElement();
            Add(content);
            UseContent(content);
        }
    }
}
