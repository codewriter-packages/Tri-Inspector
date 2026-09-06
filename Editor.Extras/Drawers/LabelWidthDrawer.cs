using TriInspector.VisualElements;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Decorator)]
    public class LabelWidthDrawer : TriAttributeDrawer<LabelWidthAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriLabelWidthContextVisualElement(Attribute.Width, next);
        }
    }
}
