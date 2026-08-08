using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(LabelWidthDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class LabelWidthDrawer : TriAttributeDrawer<LabelWidthAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriLabelWidthContextVisualElement(Attribute.Width, next);
        }
    }
}
