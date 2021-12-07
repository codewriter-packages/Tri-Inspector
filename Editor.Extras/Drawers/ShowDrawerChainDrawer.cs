using System.Collections.Generic;
using System.Text;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;

[assembly: RegisterTriDrawer(typeof(ShowDrawerChainDrawer), TriDrawerOrder.System)]

namespace TriInspector.Drawers
{
    public class ShowDrawerChainDrawer : TriCustomDrawer
    {
        public override TriElement CreateElementInternal(TriProperty property, TriElement next)
        {
            return new TriDrawerChainInfoElement(property.AllDrawers);
        }
    }

    public class TriDrawerChainInfoElement : TriElement
    {
        public TriDrawerChainInfoElement(IReadOnlyList<TriCustomDrawer> drawers)
        {
            var info = new StringBuilder();

            info.Append("Drawer Chain:");

            for (var i = 0; i < drawers.Count; i++)
            {
                var drawer = drawers[i];
                info.AppendLine();
                info.Append(i).Append(": ").Append(drawer.GetType().Name);
            }

            AddChild(new TriInfoBoxElement(info.ToString()));
        }
    }
}