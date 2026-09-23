using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

namespace TriInspector.GroupDrawers
{
    [RegisterTriGroupDrawer]
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