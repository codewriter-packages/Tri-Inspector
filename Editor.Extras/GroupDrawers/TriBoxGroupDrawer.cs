using TriInspector;
using TriInspector.GroupDrawers;
using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

[assembly: RegisterTriGroupDrawer(typeof(TriBoxGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriBoxGroupDrawer : TriGroupDrawer<DeclareBoxGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareBoxGroupAttribute attribute)
        {
            if (attribute.HideTitle)
            {
                return new TriBoxGroupVisualElement(hideIfChildrenInvisible: true);
            }

            return new TriHeaderBoxGroupVisualElement(attribute.Title, hideIfChildrenInvisible: true);
        }
    }
}