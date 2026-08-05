using TriInspector;
using TriInspector.GroupDrawers;
using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

[assembly: RegisterTriGroupDrawer(typeof(TriFoldoutGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriFoldoutGroupDrawer : TriGroupDrawer<DeclareFoldoutGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareFoldoutGroupAttribute attribute)
        {
            return new TriFoldoutGroupVisualElement(attribute.Title, attribute.Expanded, hideIfChildrenInvisible: true);
        }
    }
}