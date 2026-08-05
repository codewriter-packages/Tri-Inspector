using TriInspector;
using TriInspector.GroupDrawers;
using TriInspector.VisualElements;
using TriInspector.VisualElements.Groups;

[assembly: RegisterTriGroupDrawer(typeof(TriTabGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriTabGroupDrawer : TriGroupDrawer<DeclareTabGroupAttribute>
    {
        public override TriPropertyCollectionVisualElement CreateVisualElement(DeclareTabGroupAttribute attribute)
        {
            return new TriTabGroupVisualElement();
        }
    }
}