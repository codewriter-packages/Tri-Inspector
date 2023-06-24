using TriInspector;
using TriInspector.Elements;
using TriInspector.GroupDrawers;

[assembly: RegisterTriGroupDrawer(typeof(TriToggleGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriToggleGroupDrawer : TriGroupDrawer<DeclareToggleGroupAttribute>
    {
        public override TriPropertyCollectionBaseElement CreateElement(DeclareToggleGroupAttribute attribute)
        {
            return new TriBoxGroupElement(new TriBoxGroupElement.Props
            {
                title = attribute.Title,
                titleMode = TriBoxGroupElement.TitleMode.Toggle,
                expandedByDefault = attribute.Collapsible,
                hideIfChildrenInvisible = true,
            });
        }
    }
}