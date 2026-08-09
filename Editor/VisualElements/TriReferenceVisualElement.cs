using System;
using TriInspector.Utilities;
using UnityEditor;
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
                root.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    property.ValueChanged += OnValueChanged;
                    OnValueChanged(property);
                });
                root.RegisterCallback<DetachFromPanelEvent>(_ => property.ValueChanged -= OnValueChanged);
            }

            if (props.inline)
            {
                var inlineRoot = new VisualElement();

                if (showReferencePicker)
                {
                    inlineRoot.Add(CreateTypeSelectorIsland());
                }

                RebuildContent();
                inlineRoot.Add(content);

                if (props.drawPrefixLabel)
                {
                    inlineRoot = new TriAlignedLabelVisualElement(property, inlineRoot);
                }

                BindLifecycle(inlineRoot);
                Add(inlineRoot);
                return;
            }

            var foldout = new Foldout
            {
                value = property.IsExpanded,
            };

            foldout.SetAcceptClicksIfDisabled(true);
            foldout.AutoSyncLabelFromProperty(property);

            if (showReferencePicker)
            {
                TriAlignedLabelVisualElement.InjectAlignedLabelFieldIntoFoldout(foldout, CreateTypeSelectorIsland());
            }

            if (property.IsExpanded)
            {
                RebuildContent();
            }

            foldout.RegisterValueChangedCallback(evt =>
            {
                // Foldout also bubbles ChangeEvent<bool> from child toggles; only react to its own.
                if (evt.target != foldout)
                {
                    return;
                }

                property.IsExpanded = evt.newValue;

                if (evt.newValue && !hasBuilt)
                {
                    RebuildContent();
                }
            });

            foldout.Add(content);
            BindLifecycle(foldout);
            Add(foldout);
        }

        private IMGUIContainer CreateTypeSelectorIsland()
        {
            // The managed-reference picker is an IMGUI AdvancedDropdown with no native equivalent
            return new IMGUIContainer(() =>
            {
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                TriManagedReferenceGui.DrawTypeSelector(rect, _property);
            });
        }
    }
}