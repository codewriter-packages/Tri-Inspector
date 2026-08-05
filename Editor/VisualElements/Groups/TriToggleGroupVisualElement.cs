using UnityEngine.UIElements;

namespace TriInspector.VisualElements.Groups
{
    public class TriToggleGroupVisualElement : TriBoxGroupBaseVisualElement
    {
        private readonly bool _collapsible;
        private readonly Toggle _toggle;

        private TriProperty _toggleProperty;

        public TriToggleGroupVisualElement(string title, bool collapsible, bool hideIfChildrenInvisible)
            : base(title, hideIfChildrenInvisible)
        {
            _collapsible = collapsible;

            var header = new VisualElement();
            header.AddToClassList(TriStyles.BoxGroupHeader);

            _toggle = new Toggle();
            _toggle.RegisterValueChangedCallback(evt =>
            {
                _toggleProperty?.SetValue(evt.newValue);
                ApplyToggleState(evt.newValue);
            });
            header.Add(_toggle);

            Add(header);

            var content = new VisualElement();
            Add(content);
            UseContent(content);
        }

        protected override bool TryConsumeProperty(TriProperty property)
        {
            if (_toggleProperty == null)
            {
                if (property.ValueType == typeof(bool))
                {
                    // The bool itself becomes the header toggle; it is not shown in the content.
                    _toggleProperty = property;
                    return true;
                }

                if (property.ChildrenProperties != null && property.ChildrenProperties.Count > 0 &&
                    property.ChildrenProperties[0].ValueType == typeof(bool))
                {
                    _toggleProperty = property.ChildrenProperties[0];
                }
            }

            return false;
        }

        protected override void OnSync(string title)
        {
            if (_toggleProperty?.Value is bool value)
            {
                _toggle.text = title;
                _toggle.SetValueWithoutNotify(value);
                ApplyToggleState(value);
            }
            else
            {
                _toggle.text = "The first property in the group must be of bool.";
            }
        }

        private void ApplyToggleState(bool on)
        {
            if (_collapsible)
            {
                Content.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
            {
                Content.SetEnabled(on);
            }
        }
    }
}