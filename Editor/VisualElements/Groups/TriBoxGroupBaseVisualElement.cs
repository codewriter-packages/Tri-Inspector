using System.Collections.Generic;
using TriInspector.Resolvers;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements.Groups
{
    public abstract class TriBoxGroupBaseVisualElement : TriPropertyCollectionVisualElement
    {
        private readonly bool _hideIfChildrenInvisible;
        private readonly string _titleExpression;
        private readonly List<TriProperty> _properties = new List<TriProperty>();

        private ValueResolver<string> _titleResolver;
        private TriProperty _firstProperty;

        protected TriBoxGroupBaseVisualElement(string titleExpression, bool hideIfChildrenInvisible)
        {
            _titleExpression = titleExpression;
            _hideIfChildrenInvisible = hideIfChildrenInvisible;

            AddToClassList(TriStyles.BoxGroup);

            RegisterCallback<AttachToPanelEvent>(_ => Sync());
            this.PeriodicRun(Sync);
        }

        protected VisualElement Content { get; private set; }

        protected void UseContent(VisualElement content)
        {
            Content = content;
            Content.AddToClassList(TriStyles.BoxGroupContent);
        }

        protected virtual bool TryConsumeProperty(TriProperty property)
        {
            return false;
        }

        protected virtual void OnSync(string title)
        {
        }

        protected override void AddPropertyChild(VisualElement child, TriProperty property)
        {
            _properties.Add(property);

            if (_firstProperty == null)
            {
                _firstProperty = property;
                _titleResolver = ValueResolver.ResolveString(property.Definition, _titleExpression ?? "");

                if (_titleResolver.TryGetErrorString(out var error))
                {
                    Content.Add(new TriInfoBoxVisualElement(error, TriMessageType.Error));
                }
            }

            if (TryConsumeProperty(property))
            {
                return;
            }

            Content.Add(child);
        }

        private void Sync()
        {
            if (_hideIfChildrenInvisible)
            {
                var anyVisible = false;
                for (var i = 0; i < _properties.Count; i++)
                {
                    if (_properties[i].IsVisible)
                    {
                        anyVisible = true;
                        break;
                    }
                }

                style.display = anyVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var title = _titleResolver != null && _firstProperty != null
                ? _titleResolver.GetValue(_firstProperty)
                : _titleExpression;

            OnSync(title);
        }
    }
}
