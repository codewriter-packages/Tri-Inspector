using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(InlineButtonDrawer), TriDrawerOrder.Decorator - 100)]

namespace TriInspector.Drawers
{
    public class InlineButtonDrawer : TriAttributeDrawer<InlineButtonAttribute>
    {
        private ActionResolver _actionResolver;
        private ValueResolver<bool> _isCheckedResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _actionResolver = ActionResolver.Resolve(propertyDefinition, Attribute.Name);
            if (_actionResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            if (!string.IsNullOrEmpty(Attribute.IsChecked))
            {
                _isCheckedResolver = ValueResolver.Resolve<bool>(propertyDefinition, Attribute.IsChecked);
                if (_isCheckedResolver.TryGetErrorString(out var checkedError))
                {
                    return checkedError;
                }
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var buttonText = !string.IsNullOrEmpty(Attribute.ButtonLabel)
                ? Attribute.ButtonLabel
                : Attribute.Name;

            VisualElement trailing;
            if (_isCheckedResolver != null)
            {
                trailing = new TriInlineToggle(property, buttonText, _actionResolver, _isCheckedResolver);
            }
            else
            {
                var button = new Button(() => _actionResolver.InvokeForAllTargets(property))
                {
                    text = buttonText,
                };
                button.AddToClassList(Styles.Button);
                trailing = button;
            }

            return new TriInlineButtonRow(next, trailing, Attribute);
        }

        private class TriInlineButtonRow : VisualElement
        {
            public TriInlineButtonRow(VisualElement next, VisualElement trailing, InlineButtonAttribute attribute)
            {
                AddToClassList(Styles.Row);

                next.AddToClassList(Styles.Field);
                Add(next);

                if (attribute.ButtonWidth > 0f)
                {
                    trailing.style.width = attribute.ButtonWidth;
                }

                Add(trailing);
            }
        }

        private sealed class TriInlineToggle : ToggleButtonGroup
        {
            private readonly TriProperty _property;
            private readonly ActionResolver _actionResolver;
            private readonly ValueResolver<bool> _isCheckedResolver;

            public TriInlineToggle(TriProperty property, string text,
                ActionResolver actionResolver, ValueResolver<bool> isCheckedResolver)
            {
                _property = property;
                _actionResolver = actionResolver;
                _isCheckedResolver = isCheckedResolver;

                AddToClassList(Styles.ToggleGroup);

                label = null;
                isMultipleSelection = false;
                allowEmptySelection = true;

                var button = new Button {text = text};
                button.AddToClassList(Styles.ToggleButton);
                Add(button);

                SetValueWithoutNotify(MakeState(isCheckedResolver.GetValue(property)));

                this.RegisterValueChangedCallback(OnClicked);
                this.PeriodicRun(SyncChecked);
            }

            private void OnClicked(ChangeEvent<ToggleButtonGroupState> evt)
            {
                _actionResolver.InvokeForAllTargets(_property);
                SyncChecked();
            }

            private void SyncChecked()
            {
                var current = _isCheckedResolver.GetValue(_property);
                if (value[0] != current)
                {
                    SetValueWithoutNotify(MakeState(current));
                }
            }

            private static ToggleButtonGroupState MakeState(bool on)
            {
                return new ToggleButtonGroupState(0, 1)
                {
                    [0] = on,
                };
            }
        }

        private static class Styles
        {
            public const string Row = "tri-inline-button__row";
            public const string Field = "tri-inline-button__field";
            public const string Button = "tri-inline-button__button";
            public const string ToggleGroup = "tri-inline-button__toggle-group";
            public const string ToggleButton = "tri-inline-button__toggle-button";
        }
    }
}
