using System;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriReferenceVisualElement : VisualElement
    {
        private readonly TriProperty _property;

        [Serializable]
        public struct Props
        {
            public bool inline;
            public bool drawPrefixLabel;
            public float labelWidth;
        }

        public TriReferenceVisualElement(TriProperty property, Props props = default)
        {
            _property = property;
            var showReferencePicker = !property.TryGetAttribute(out HideReferencePickerAttribute _);

            var content = new VisualElement();
            var builtType = default(Type);
            var hasBuilt = false;

            void RebuildContent()
            {
                hasBuilt = true;
                builtType = property.ValueType;

                VisualElement child = new TriPropertyCollectionVisualElement(builtType, property.ChildrenProperties);

                child = new TriLabelWidthContextVisualElement(props.labelWidth, child);

                content.Clear();
                content.Add(child);
            }

            void OnValueChanged(TriProperty changed)
            {
                if (hasBuilt && property.ValueType != builtType)
                {
                    RebuildContent();
                }
            }

            void BindLifecycle(VisualElement root)
            {
                root.TrackPropertyValueChanged(property, OnValueChanged);
            }

            if (props.inline)
            {
                var inlineRoot = new VisualElement();

                if (showReferencePicker)
                {
                    inlineRoot.Add(CreateTypeSelector());
                }

                RebuildContent();
                inlineRoot.Add(content);

                if (props.drawPrefixLabel)
                {
                    inlineRoot = new TriAlignedLabelForGenericVisualElement(property, inlineRoot);
                }

                BindLifecycle(inlineRoot);
                Add(inlineRoot);
                return;
            }

            var header = new TriAlignedLabelForGenericVisualElement(
                property, showReferencePicker ? CreateTypeSelector() : new VisualElement(), collapsible: true);

            content.AddToClassList(TriStyles.ReferenceContent);

            void SetContentVisible(bool contentVisible)
            {
                content.style.display = contentVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (property.IsExpanded)
            {
                RebuildContent();
            }

            SetContentVisible(property.IsExpanded);

            header.Foldout.RegisterValueChangedCallback(evt =>
            {
                // Foldout also bubbles ChangeEvent<bool> from child toggles; only react to its own.
                if (evt.target != header.Foldout)
                {
                    return;
                }

                property.IsExpanded = evt.newValue;

                if (evt.newValue && !hasBuilt)
                {
                    RebuildContent();
                }

                SetContentVisible(evt.newValue);
            });

            Add(header);
            Add(content);
            BindLifecycle(this);
        }

        private VisualElement CreateTypeSelector()
        {
            return new TriReferenceTypeSelectorVisualElement(_property)
            {
                style =
                {
                    marginRight = -2,
                }
            };
        }
    }
}