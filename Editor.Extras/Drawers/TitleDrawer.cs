using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(TitleDrawer), TriDrawerOrder.Inspector)]

namespace TriInspector.Drawers
{
    public class TitleDrawer : TriAttributeDrawer<TitleAttribute>
    {
        private ValueResolver<string> _titleResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _titleResolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Title);

            if (_titleResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriTitle(property, next, Attribute, _titleResolver);
        }

        private class TriTitle : VisualElement
        {
            public TriTitle(TriProperty property, VisualElement next,
                TitleAttribute attribute, ValueResolver<string> titleResolver)
            {
                var title = new Label();
                title.AddToClassList(Styles.Title);
                Add(title);

                if (attribute.HorizontalLine)
                {
                    var line = new VisualElement();
                    line.AddToClassList(Styles.Line);
                    Add(line);
                }

                Add(next);

                this.TrackResolvedValue(property, titleResolver, "", value => title.text = value);
            }
        }

        private static class Styles
        {
            public const string Title = "tri-title";
            public const string Line = "tri-title__line";
        }
    }
}