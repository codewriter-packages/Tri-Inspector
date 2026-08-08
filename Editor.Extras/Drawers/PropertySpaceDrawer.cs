using TriInspector;
using TriInspector.Drawers;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(PropertySpaceDrawer), TriDrawerOrder.Inspector)]

namespace TriInspector.Drawers
{
    public class PropertySpaceDrawer : TriAttributeDrawer<PropertySpaceAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriPropertySpace(next)
            {
                style =
                {
                    marginTop = Attribute.SpaceBefore,
                    marginBottom = Attribute.SpaceAfter,
                },
            };
        }

        private class TriPropertySpace : VisualElement
        {
            public TriPropertySpace(VisualElement next)
            {
                Add(next);
            }
        }
    }
}