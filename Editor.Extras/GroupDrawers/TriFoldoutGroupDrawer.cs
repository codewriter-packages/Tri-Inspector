using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

namespace TriInspector.GroupDrawers
{
    [RegisterTriGroupDrawer]
    public class TriFoldoutGroupDrawer : TriGroupDrawer<DeclareFoldoutGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareFoldoutGroupAttribute attribute)
        {
            return new TriFoldoutGroupVisualElement(attribute.Title, attribute.Expanded, hideIfChildrenInvisible: true);
        }
    }
}