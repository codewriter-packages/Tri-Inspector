using TriInspector;
using TriInspector.GroupDrawers;
using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

[assembly: RegisterTriGroupDrawer(typeof(TriToggleGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriToggleGroupDrawer : TriGroupDrawer<DeclareToggleGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareToggleGroupAttribute attribute)
        {
            return new TriToggleGroupVisualElement(attribute.Title, attribute.Collapsible,
                hideIfChildrenInvisible: true);
        }
    }
}