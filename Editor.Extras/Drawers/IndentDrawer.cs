using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Decorator)]
    public class IndentDrawer : TriAttributeDrawer<IndentAttribute>
    {
        private const float IndentWidth = 15f;

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            next.style.marginLeft = Attribute.Indent * IndentWidth - 1;
            return next;
        }
    }
}
