using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriPropertyVisualElement : VisualElement
    {
        [Serializable]
        public struct Props
        {
            public bool forceInline;
        }

        public TriPropertyVisualElement(TriProperty property, Props props = default)
        {
            foreach (var error in property.ExtensionErrors)
            {
                Add(new TriInfoBoxVisualElement(error, TriMessageType.Error));
            }

            var element = CreateElement(property, props);

            var drawers = property.AllDrawers;
            for (var index = drawers.Count - 1; index >= 0; index--)
            {
                element = drawers[index].CreateVisualElement(property, element) ??
                          new TriInfoBoxVisualElement($"Drawer {drawers[index].GetType().Name} is not implemented",
                              TriMessageType.Error);
            }

            Add(element);

            // Refresh the property when its value changes outside of TriInspector (Undo, another
            // inspector, animation, a bound native field, a script). Serialized properties can be
            // tracked event-driven; non-serialized ones have to be polled.
            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                this.TrackPropertyValue(serializedProperty, _ => property.RefreshValue());
            }
            else
            {
                // this.PeriodicRun(property.RefreshValue);
            }

            void Sync()
            {
                element.style.display = property.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
                element.SetEnabled(property.IsEnabled);
            }

            Sync();
            element.PeriodicRun(Sync);
        }

        private static VisualElement CreateElement(TriProperty property, Props props)
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
                    return new TriNoDrawerVisualElement(property);
                }
            }
        }

        private static VisualElement CreateArrayElement(TriProperty property)
        {
            return new TriListVisualElement(property);
        }

        private static VisualElement CreateReferenceElement(TriProperty property, Props props)
        {
            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new TriReferenceVisualElement(property, new TriReferenceVisualElement.Props
                {
                    inline = true,
                    drawPrefixLabel = !props.forceInline,
                    labelWidth = inlineAttribute.LabelWidth,
                });
            }

            if (props.forceInline)
            {
                return new TriReferenceVisualElement(property, new TriReferenceVisualElement.Props
                {
                    inline = true,
                    drawPrefixLabel = false,
                });
            }

            return new TriReferenceVisualElement(property, new TriReferenceVisualElement.Props
            {
                inline = false,
                drawPrefixLabel = false,
            });
        }

        private static VisualElement CreateGenericElement(TriProperty property, Props props)
        {
            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new TriInlineGenericVisualElement(property, new TriInlineGenericVisualElement.Props
                {
                    drawPrefixLabel = !props.forceInline,
                    labelWidth = inlineAttribute.LabelWidth,
                });
            }

            if (props.forceInline)
            {
                return new TriInlineGenericVisualElement(property, new TriInlineGenericVisualElement.Props
                {
                    drawPrefixLabel = false,
                });
            }

            return new TriFoldoutVisualElement(property);
        }
    }
}