using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    public class TriPropertyElement : TriElement
    {
        private readonly TriProperty _property;

        private readonly TriElement _wrappedElement;

        [Serializable]
        public struct Props
        {
            public bool forceInline;
        }

        public TriPropertyElement(TriProperty property, Props props = default)
        {
            _property = property;

            foreach (var error in _property.ExtensionErrors)
            {
                AddChild(new TriInfoBoxElement(error, TriMessageType.Error));
            }

            var element = CreateElement(property, props);

            var drawers = property.AllDrawers;
            for (var index = drawers.Count - 1; index >= 0; index--)
            {
                element = drawers[index].CreateElementInternal(property, element);
            }

            _wrappedElement = element;

            AddChild(element);
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            var canRecurse = _property.ExtensionErrors.Count == 0
                             && TriNativeProjection.IsNative(_wrappedElement);

            if (!canRecurse)
            {
                return base.CreateVisualElement(property);
            }

            var visualElement = _wrappedElement.CreateVisualElement(property);

            void Sync()
            {
                visualElement.style.display = _property.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
                visualElement.SetEnabled(_property.IsEnabled);
            }

            Sync();
            visualElement.schedule.Execute(Sync).Every(100);

            return visualElement;
        }

        public override float GetHeight(float width)
        {
            if (!_property.IsVisible)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            return base.GetHeight(width);
        }

        public override void OnGUI(Rect position)
        {
            if (!_property.IsVisible)
            {
                return;
            }

            var oldShowMixedValue = EditorGUI.showMixedValue;
            var oldEnabled = GUI.enabled;

            GUI.enabled &= _property.IsEnabled;
            EditorGUI.showMixedValue = _property.IsValueMixed;

            if (_property.TryGetSerializedProperty(out var serializedProperty))
            {
                EditorGUI.BeginProperty(position, null, serializedProperty);
            }

            base.OnGUI(position);

            if (_property.TryGetSerializedProperty(out _))
            {
                EditorGUI.EndProperty();
            }

            EditorGUI.showMixedValue = oldShowMixedValue;
            GUI.enabled = oldEnabled;
        }

        private static TriElement CreateElement(TriProperty property, Props props)
        {
            switch (property.PropertyType)
            {
                case TriPropertyType.Array:
                {
                    return CreateArrayElement(property);
                }

                case TriPropertyType.Reference:
                {
                    return CreateReferenceElement(property, props);
                }

                case TriPropertyType.Generic:
                {
                    return CreateGenericElement(property, props);
                }

                default:
                {
                    return new TriNoDrawerElement(property);
                }
            }
        }

        private static TriElement CreateArrayElement(TriProperty property)
        {
            return new TriListElement(property);
        }

        private static TriElement CreateReferenceElement(TriProperty property, Props props)
        {
            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new TriReferenceElement(property, new TriReferenceElement.Props
                {
                    inline = true,
                    drawPrefixLabel = !props.forceInline,
                    labelWidth = inlineAttribute.LabelWidth,
                });
            }

            if (props.forceInline)
            {
                return new TriReferenceElement(property, new TriReferenceElement.Props
                {
                    inline = true,
                    drawPrefixLabel = false,
                });
            }

            return new TriReferenceElement(property, new TriReferenceElement.Props
            {
                inline = false,
                drawPrefixLabel = false,
            });
        }

        private static TriElement CreateGenericElement(TriProperty property, Props props)
        {
            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new TriInlineGenericElement(property, new TriInlineGenericElement.Props
                {
                    drawPrefixLabel = !props.forceInline,
                    labelWidth = inlineAttribute.LabelWidth,
                });
            }

            if (props.forceInline)
            {
                return new TriInlineGenericElement(property, new TriInlineGenericElement.Props
                {
                    drawPrefixLabel = false,
                });
            }

            return new TriFoldoutElement(property);
        }
    }
}