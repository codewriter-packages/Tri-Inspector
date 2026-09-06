using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

namespace TriInspector.GroupDrawers
{
    [RegisterTriGroupDrawer]
    public class TriTabGroupDrawer : TriGroupDrawer<DeclareTabGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareTabGroupAttribute attribute)
        {
            return new TriTabGroupVisualElement();
        }
    }
}