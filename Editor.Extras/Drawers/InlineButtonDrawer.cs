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

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _actionResolver = ActionResolver.Resolve(propertyDefinition, Attribute.Name);
            if (_actionResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriInlineButtonRow(property, next, Attribute, _actionResolver);
        }

        private class TriInlineButtonRow : VisualElement
        {
            public TriInlineButtonRow(TriProperty property, VisualElement next,
                InlineButtonAttribute attribute, ActionResolver actionResolver)
            {
                var buttonText = !string.IsNullOrEmpty(attribute.ButtonLabel)
                    ? attribute.ButtonLabel
                    : attribute.Name;

                AddToClassList(Styles.Row);

                next.AddToClassList(Styles.Field);
                Add(next);

                var button = new Button(() => actionResolver.InvokeForAllTargets(property))
                {
                    text = buttonText,
                };
                button.AddToClassList(Styles.Button);

                if (attribute.ButtonWidth > 0f)
                {
                    button.style.width = attribute.ButtonWidth;
                }

                Add(button);
            }
        }

        private static class Styles
        {
            public const string Row = "tri-inline-button__row";
            public const string Field = "tri-inline-button__field";
            public const string Button = "tri-inline-button__button";
        }
    }
}