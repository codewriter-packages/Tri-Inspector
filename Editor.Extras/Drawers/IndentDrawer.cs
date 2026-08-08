using TriInspector;
using TriInspector.Drawers;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(IndentDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
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
