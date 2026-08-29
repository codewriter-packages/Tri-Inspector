using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriNoDrawerVisualElement : VisualElement
    {
        public TriNoDrawerVisualElement(TriProperty property)
        {
            var label = new Label($"No drawer for {property.FieldType}");
            label.AddToClassList(TriStyles.NoDrawerLabel);
            Add(new TriAlignedLabelVisualElement<object>(property, label));
        }
    }
}