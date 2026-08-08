using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly:
    RegisterTriAttributeDrawer(typeof(DisplayAsStringDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class DisplayAsStringDrawer : TriAttributeDrawer<DisplayAsStringAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriAlignedLabelVisualElement(property, new TriDisplayAsString(property));
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