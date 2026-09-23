using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

namespace TriInspector.GroupDrawers
{
    [RegisterTriGroupDrawer]
    public class TriVerticalGroupDrawer : TriGroupDrawer<DeclareVerticalGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareVerticalGroupAttribute attribute)
        {
            return new TriVerticalGroupVisualElement();
        }
    }
}