using TriInspector;
using TriInspector.Elements;
using TriInspector.GroupDrawers;

[assembly: RegisterTriGroupDrawer(typeof(TriFoldoutGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriFoldoutGroupDrawer : TriGroupDrawer<DeclareFoldoutGroupAttribute>
    {
        public override TriPropertyCollectionBaseElement CreateElement(DeclareFoldoutGroupAttribute attribute)
        {
            return new TriBoxGroupElement(attribute.Title, new TriBoxGroupElement.Props
            {
                foldout = true,
                expandedByDefault = attribute.Expanded,
            });
        }
    }
}