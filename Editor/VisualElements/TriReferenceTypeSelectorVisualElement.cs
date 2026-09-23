using TriInspector.Utilities;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriReferenceTypeSelectorVisualElement : VisualElement
    {
        private readonly TriProperty _property;
        private readonly TextElement _text;

        public TriReferenceTypeSelectorVisualElement(TriProperty property)
        {
            _property = property;

            AddToClassList("unity-base-field__input");
            AddToClassList("unity-base-popup-field__input");
            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Row;

            _text = new TextElement();
            _text.AddToClassList("unity-base-popup-field__text");
            Add(_text);

            var arrow = new VisualElement();
            arrow.AddToClassList("unity-base-popup-field__arrow");
            Add(arrow);

            RegisterCallback<ClickEvent>(_ => TriManagedReferenceGui.ShowTypeDropdown(worldBound, _property));

            this.TrackPropertyValueChanged(property, _ => UpdateText());
        }

        private void UpdateText()
        {
            _text.text = _property.ValueType != null
                ? TriTypeUtilities.GetTypeNiceName(_property.ValueType)
                : "[None]";
        }
    }
}
