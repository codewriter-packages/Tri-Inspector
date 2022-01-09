using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    internal class TriPropertyElement : TriElement
    {
        private readonly TriProperty _property;

        public TriPropertyElement(TriProperty property, bool inline = false)
        {
            _property = property;

            var element = CreateElement(property, inline);

            var drawers = property.AllDrawers;
            for (var index = drawers.Count - 1; index >= 0; index--)
            {
                if (_property.IsArrayElement && !drawers[index].ApplyOnArrayElement ||
                    _property.IsArray && drawers[index].ApplyOnArrayElement)
                {
                    continue;
                }

                element = drawers[index].CreateElementInternal(property, element);
            }

            AddChild(element);
        }

        public override float GetHeight(float width)
        {
            if (!_property.IsVisible)
            {
                return 0f;
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

            base.OnGUI(position);

            EditorGUI.showMixedValue = oldShowMixedValue;
            GUI.enabled = oldEnabled;
        }

        private static TriElement CreateElement(TriProperty property, bool inline = false)
        {
            var isSerializedProperty = property.TryGetSerializedProperty(out var serializedProperty);

            if (!isSerializedProperty && property.PropertyTree.TargetObjects.Length > 1)
            {
                return new TriMultiEditNotSupportedElement(property);
            }

            var handler = isSerializedProperty
                ? ScriptAttributeUtilityProxy.GetHandler(serializedProperty)
                : default(PropertyHandlerProxy?);

            if (!handler.HasValue || !handler.Value.hasPropertyDrawer)
            {
                var propertyType = property.PropertyType;

                switch (propertyType)
                {
                    case TriPropertyType.Array:
                    {
                        return CreateArrayElement(property);
                    }

                    case TriPropertyType.Reference:
                    {
                        return CreateReferenceElement(property, inline);
                    }

                    case TriPropertyType.Generic:
                    {
                        return CreateGenericElement(property, inline);
                    }
                }
            }

            if (isSerializedProperty)
            {
                return new TriBuiltInPropertyElement(property, serializedProperty, handler.Value);
            }

            return new TriNoDrawerElement(property);
        }

        private static TriElement CreateArrayElement(TriProperty property)
        {
            return new TriListElement(property);
        }

        private static TriElement CreateReferenceElement(TriProperty property, bool inline)
        {
            if (inline)
            {
                return new TriReferenceElement(property, true);
            }

            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new TriReferenceElement(property, true, true, inlineAttribute.LabelWidth);
            }

            return new TriReferenceElement(property);
        }

        private static TriElement CreateGenericElement(TriProperty property, bool inline)
        {
            if (inline)
            {
                return new TriInlineGenericElement(property);
            }

            if (property.TryGetAttribute(out InlinePropertyAttribute inlineAttribute))
            {
                return new TriInlineGenericElement(property, true, inlineAttribute.LabelWidth);
            }

            return new TriFoldoutElement(property);
        }
    }
}