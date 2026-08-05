using TriInspector;
using TriInspector.GroupDrawers;
using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

[assembly: RegisterTriGroupDrawer(typeof(TriHorizontalGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriHorizontalGroupDrawer : TriGroupDrawer<DeclareHorizontalGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareHorizontalGroupAttribute attribute)
        {
            return new TriHorizontalGroupVisualElement(attribute.Sizes);
        }
    }
}