using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriListVisualElement : TriCollectionVisualElement
    {
        private readonly bool _inlineElements;

        public TriListVisualElement(TriProperty property, bool inlineElements = true) : base(property)
        {
            _inlineElements = inlineElements;

            if (inlineElements)
            {
                var labelOverride = new ListPropertyOverrideContext(property);
                RegisterCallback<AttachToPanelEvent>(_ => property.PropertyTree.AddPropertyOverride(labelOverride));
                RegisterCallback<DetachFromPanelEvent>(_ =>
                    property.PropertyTree.RemovePropertyOverride(labelOverride));
            }
        }

        protected override VisualElement CreateItemElement(TriProperty property)
        {
            return new TriPropertyVisualElement(property, new TriPropertyVisualElement.Props
            {
                forceInline = _inlineElements,
            });
        }

        private class ListPropertyOverrideContext : TriPropertyOverrideContext
        {
            private readonly TriProperty _listProperty;
            private readonly GUIContent _noneLabel = GUIContent.none;

            public ListPropertyOverrideContext(TriProperty listProperty)
            {
                _listProperty = listProperty;
            }

            public override bool TryGetDisplayName(TriProperty property, out GUIContent displayName)
            {
                if (property.Parent == _listProperty)
                {
                    displayName = _noneLabel;
                    return true;
                }

                displayName = default;
                return false;
            }
        }
    }
}