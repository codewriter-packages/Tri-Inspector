using TriInspector;
using TriInspector.Elements;
using TriInspector.GroupDrawers;

[assembly: RegisterTriGroupDrawer(typeof(TriBoxGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriBoxGroupDrawer : TriGroupDrawer<DeclareBoxGroupAttribute>
    {
        public override TriPropertyCollectionBaseElement CreateElement(DeclareBoxGroupAttribute attribute)
        {
            return new TriBoxGroupElement(attribute);
        }
    }
}