using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public abstract class TriHeaderGroupBaseElement : TriPropertyCollectionBaseElement
    {
        private readonly Props _props;
        private const float InsetTop = 4;
        private const float InsetBottom = 4;
        private const float InsetLeft = 4;
        private const float InsetRight = 4;

        private readonly List<TriProperty> _properties = new List<TriProperty>();

        private bool IsAnyPropertyVisible => _properties.Any(it => it.IsVisible);

        [Serializable]
        public struct Props
        {
            public bool hideIfChildrenInvisible;
        }

        protected TriHeaderGroupBaseElement(Props props = default)
        {
            _props = props;
        }
        
        protected override void AddPropertyChild(TriElement element, TriProperty property)
        {
            _properties.Add(property);

            base.AddPropertyChild(element, property);
        }

        protected virtual float GetHeaderHeight(float width)
        {
            return 22;
        }

        protected virtual float GetContentHeight(float width)
        {
            return base.GetHeight(width);
        }

        protected virtual void DrawHeader(Rect position)
        {
        }

        protected virtual void DrawContent(Rect position)
        {
            base.OnGUI(position);
        }

        public sealed override float GetHeight(float width)
        {
            if (_props.hideIfChildrenInvisible && !IsAnyPropertyVisible)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            var headerHeight = GetHeaderHeight(width);
            var contentHeight = GetContentHeight(width);

            var height = headerHeight + contentHeight;

            if (contentHeight > 0)
            {
                height += InsetTop + InsetBottom;
            }

            return height;
        }

        public sealed override void OnGUI(Rect position)
        {
            if (_props.hideIfChildrenInvisible && !IsAnyPropertyVisible)
            {
                return;
            }

            var headerHeight = GetHeaderHeight(position.width);
            var contentHeight = GetContentHeight(position.width);

            var headerBgRect = new Rect(position)
            {
                height = headerHeight,
            };
            var contentBgRect = new Rect(position)
            {
                yMin = headerBgRect.yMax,
            };
            var contentRect = new Rect(contentBgRect)
            {
                xMin = contentBgRect.xMin + InsetLeft,
                xMax = contentBgRect.xMax - InsetRight,
                yMin = contentBgRect.yMin + InsetTop,
                yMax = contentBgRect.yMax - InsetBottom,
                height = contentHeight,
            };

            if (headerHeight > 0f)
            {
                DrawHeader(headerBgRect);
            }

            if (contentHeight > 0)
            {
                TriEditorGUI.DrawBox(contentBgRect, headerHeight > 0f
                    ? TriEditorStyles.ContentBox
                    : TriEditorStyles.Box);

                using (TriGuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth - InsetLeft))
                {
                    DrawContent(contentRect);
                }
            }
        }
    }
}