using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Inspector)]
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