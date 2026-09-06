using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

namespace TriInspector.GroupDrawers
{
    [RegisterTriGroupDrawer]
    public class TriHorizontalGroupDrawer : TriGroupDrawer<DeclareHorizontalGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareHorizontalGroupAttribute attribute)
        {
            return new TriHorizontalGroupVisualElement(attribute.Sizes);
        }
    }
}