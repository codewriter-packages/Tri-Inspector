using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
    public class DisplayAsStringDrawer : TriAttributeDrawer<DisplayAsStringAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriAlignedLabelVisualElement<object>(property, new TriDisplayAsString(property));
        }

        private class TriDisplayAsString : Label
        {
            public TriDisplayAsString(TriProperty property)
            {
                void Sync()
                {
                    text = property.Value?.ToString() ?? "Null";
                }

                if (property.TryGetSerializedProperty(out var serializedProperty))
                {
                    this.TrackPropertyValue(serializedProperty, _ => Sync());
                }
                else
                {
                    this.PeriodicRun(Sync);
                }

                Sync();
            }
        }
    }
}