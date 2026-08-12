using System.Collections.Generic;
using System.Text;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(ShowDrawerChainDrawer), TriDrawerOrder.System)]

namespace TriInspector.Drawers
{
    public class ShowDrawerChainDrawer : TriAttributeDrawer<ShowDrawerChainAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var container = new VisualElement();
            container.Add(new TriInfoBoxVisualElement(BuildInfo(property.AllDrawers), TriMessageType.None));
            container.Add(next);
            return container;
        }

        private static string BuildInfo(IReadOnlyList<TriCustomDrawer> drawers)
        {
            var info = new StringBuilder();

            info.Append("Drawer Chain:");

            for (var i = 0; i < drawers.Count; i++)
            {
                var drawer = drawers[i];
                info.AppendLine();
                info.Append(i).Append(": ").Append(drawer.GetType().Name).Append(" - ").Append(drawer.Order);
            }

            return info.ToString();
        }
    }
}