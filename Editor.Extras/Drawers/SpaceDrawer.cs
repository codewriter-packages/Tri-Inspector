using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(8900)]
    public class SpaceDrawer : TriAttributeDrawer<SpaceAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriSpace(next, Attribute);
        }

        private class TriSpace : VisualElement
        {
            public TriSpace(VisualElement next, SpaceAttribute attribute)
            {
                style.marginTop = attribute.height;
                Add(next);
            }
        }
    }
}
