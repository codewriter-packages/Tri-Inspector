using TriInspector;
using TriInspector.GroupDrawers;
using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

[assembly: RegisterTriGroupDrawer(typeof(TriVerticalGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriVerticalGroupDrawer : TriGroupDrawer<DeclareVerticalGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareVerticalGroupAttribute attribute)
        {
            return new TriVerticalGroupVisualElement();
        }
    }
}