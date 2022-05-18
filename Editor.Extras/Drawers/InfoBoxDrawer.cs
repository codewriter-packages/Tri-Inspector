using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;

[assembly: RegisterTriAttributeDrawer(typeof(InfoBoxDrawer), TriDrawerOrder.System)]

namespace TriInspector.Drawers
{
    public class InfoBoxDrawer : TriAttributeDrawer<InfoBoxAttribute>
    {
        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            var container = new TriElement();
            container.AddChild(new TriInfoBoxElement(Attribute.Text, Attribute.MessageType));
            container.AddChild(next);
            return container;
        }
    }
}