using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TriInspector.Resolvers;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriBoxGroupElement : TriHeaderGroupBaseElement
    {
        private readonly Props _props;

        private ValueResolver<string> _headerResolver;
        private readonly List<TriProperty> _properties = new List<TriProperty>();
        private readonly Color _headerColor;
        private readonly Color _titleColor;
        private bool _expanded;
        
        [CanBeNull] private TriProperty _firstProperty;
        [CanBeNull] private TriProperty _toggleProperty;
        
        private bool HasVisibleProperties => _properties.Any(t => t.IsVisible);

        [Serializable]
        public struct Props
        {
            public string title;
            public TitleMode titleMode;
            public bool expandedByDefault;
            public bool showIfEmpty;
            public string headerColorHex;
            public string titleColorHex;
        }

        public TriBoxGroupElement(Props props = default)
        {
            _props = props;
            _expanded = _props.expandedByDefault;

            if (!ColorUtility.TryParseHtmlString(_props.headerColorHex, out _headerColor))
            {
                _headerColor = Color.clear;
            }
            if (!ColorUtility.TryParseHtmlString(_props.titleColorHex, out _titleColor))
            {
                _titleColor = Color.clear;
            }
        }

        protected override void AddPropertyChild(TriElement element, TriProperty property)
        {
            _properties.Add(property);
            
            _firstProperty = property;
            _headerResolver = ValueResolver.ResolveString(property.Definition, _props.title ?? "");
            
            if (_headerResolver.TryGetErrorString(out var error))
            {
                AddChild(new TriInfoBoxElement(error, TriMessageType.Error));
            }
            
            if (_props.titleMode == TitleMode.Toggle)
            {
                if (_toggleProperty == null)
                {
                    if (property.ValueType == typeof(bool))
                    {
                        _toggleProperty = property;

                        return;
                    }
                    
                    if (property.ChildrenProperties?.Count > 0)
                    {
                        var childrenProperty = property.ChildrenProperties[0];

                        if (childrenProperty.ValueType == typeof(bool))
                        {
                            _toggleProperty = childrenProperty;
                        }
                    }
                }
            }

            base.AddPropertyChild(element, property);
        }

        protected override float GetHeaderHeight(float width)
        {
            if (!_props.showIfEmpty && !HasVisibleProperties)
            {
                return 0f;
            }
            
            if (_props.titleMode == TitleMode.Hidden)
            {
                return 0f;
            }

            return base.GetHeaderHeight(width);
        }

        protected override float GetContentHeight(float width)
        {
            if (!_props.showIfEmpty && !HasVisibleProperties)
            {
                return 0f;
            }
            
            if (((_props.titleMode == TitleMode.Toggle && _props.expandedByDefault) || 
                 _props.titleMode == TitleMode.Foldout) && !_expanded)
            {
                return 0f;
            }

            return base.GetContentHeight(width);
        }

        protected override void DrawHeader(Rect position)
        {
            if (!_props.showIfEmpty && !HasVisibleProperties)
            {
                return;
            }

            var oldBackgroundColor = GUI.backgroundColor;
            var oldContentColor = GUI.contentColor;
            
            if (_headerColor != Color.clear)
            {
                GUI.backgroundColor = _headerColor;
            }
            if (_titleColor != Color.clear)
            {
                GUI.contentColor = _titleColor;
            }
            
            TriEditorGUI.DrawBox(position, TriEditorStyles.TabOnlyOne);

            var headerLabelRect = new Rect(position)
            {
                xMin = position.xMin + 6,
                xMax = position.xMax - 6,
                yMin = position.yMin + 2,
                yMax = position.yMax - 2,
            };

            var headerContent = _headerResolver.GetValue(_firstProperty);

            switch (_props.titleMode)
            {
                case TitleMode.Foldout:
                    headerLabelRect.x += 10;
                    _expanded = EditorGUI.Foldout(headerLabelRect, _expanded, headerContent, true);
                    break;
                case TitleMode.Toggle:
                {
                    if (_toggleProperty?.Value is bool cachedValue)
                    {
                        var newValue = EditorGUI.ToggleLeft(headerLabelRect, headerContent, cachedValue);
                    
                        if (newValue != cachedValue)
                        {
                            _toggleProperty.SetValue(newValue);
                        }
                    
                        _expanded = newValue;
                    }
                    else
                    {
                        EditorGUI.LabelField(headerLabelRect, $"The first property in the group must be of bool.");
                    }
                    break;
                }
                default:
                    EditorGUI.LabelField(headerLabelRect, headerContent);
                    break;
            }
            
            if (_headerColor != Color.clear)
            {
                GUI.backgroundColor = oldBackgroundColor;
            }
            if (_titleColor != Color.clear)
            {
                GUI.contentColor = oldContentColor;
            }
        }

        protected override void DrawContent(Rect position)
        {
            if (!_props.showIfEmpty && !HasVisibleProperties)
            {
                return;
            }
            
            if (_props.titleMode == TitleMode.Foldout && !_expanded)
            {
                return;
            }

            if (_props.titleMode == TitleMode.Toggle && !_props.expandedByDefault && !_expanded)
            {
                EditorGUI.BeginDisabledGroup(true);
                base.DrawContent(position);
                EditorGUI.EndDisabledGroup();

                return;

            }

            base.DrawContent(position);
        }

        public enum TitleMode
        {
            Normal,
            Hidden,
            Foldout,
            Toggle,
        }
    }
}