﻿using System;
using TriInspector.Utilities;
using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriPropertyElement : TriElement
    {
        private readonly TriProperty _property;

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

            AddChild(element);
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

            using (TriPropertyOverrideContext.BeginProperty())
            {
                base.OnGUI(position);
            }

            EditorGUI.showMixedValue = oldShowMixedValue;
            GUI.enabled = oldEnabled;
        }

        private static TriElement CreateElement(TriProperty property, Props props)
        {
            var isSerializedProperty = property.TryGetSerializedProperty(out var serializedProperty);

            var handler = isSerializedProperty
                ? ScriptAttributeUtilityProxy.GetHandler(serializedProperty)
                : default(PropertyHandlerProxy?);

            var drawWithUnity = handler.HasValue && handler.Value.hasPropertyDrawer ||
                                handler.HasValue && TriUnityInspectorUtilities.MustDrawWithUnity(property);

            if (!drawWithUnity)
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
                        return CreateReferenceElement(property, props);
                    }

                    case TriPropertyType.Generic:
                    {
                        return CreateGenericElement(property, props);
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