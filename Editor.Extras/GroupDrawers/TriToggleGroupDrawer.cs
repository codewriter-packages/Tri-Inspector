using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

namespace TriInspector.GroupDrawers
{
    [RegisterTriGroupDrawer]
    public class TriToggleGroupDrawer : TriGroupDrawer<DeclareToggleGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareToggleGroupAttribute attribute)
        {
            return new TriToggleGroupVisualElement(attribute.Title, attribute.Collapsible,
                hideIfChildrenInvisible: true);
        }
    }
}