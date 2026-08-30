using TriInspector;
using TriInspector.Drawers;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(HeaderDrawer), 8900)]

namespace TriInspector.Drawers
{
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