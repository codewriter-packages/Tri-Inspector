using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(8900)]
    public class HeaderDrawer : TriAttributeDrawer<HeaderAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriHeader(next, Attribute);
        }

        private class TriHeader : VisualElement
        {
            public TriHeader(VisualElement next, HeaderAttribute attribute)
            {
                var title = new Label(attribute.header)
                {
                    style =
                    {
                        marginTop = 13,
                        unityFontStyleAndWeight = FontStyle.Bold,
                    },
                };
                Add(title);
                Add(next);
            }
        }
    }
}