using TriInspector;
using TriInspector.Drawers;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(SpaceDrawer), 8900)]

namespace TriInspector.Drawers
{
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
