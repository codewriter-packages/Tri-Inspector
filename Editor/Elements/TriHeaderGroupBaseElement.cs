using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public abstract class TriHeaderGroupBaseElement : TriPropertyCollectionBaseElement
    {
        private const float InsetTop = 4;
        private const float InsetBottom = 4;
        private const float InsetLeft = 4;
        private const float InsetRight = 4;

        protected virtual float GetHeaderHeight(float width)
        {
            return 22;
        }

        protected virtual void DrawHeader(Rect position)
        {
        }

        public sealed override float GetHeight(float width)
        {
            return base.GetHeight(width) + InsetTop + InsetBottom + GetHeaderHeight(width);
        }

        public sealed override void OnGUI(Rect position)
        {
            var headerHeight = GetHeaderHeight(position.width);

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
            };

            if (headerHeight > 0f)
            {
                DrawHeader(headerBgRect);

                TriEditorGUI.DrawBox(contentBgRect, TriEditorStyles.ContentBox);
            }
            else
            {
                TriEditorGUI.DrawBox(contentBgRect, TriEditorStyles.Box);
            }

            using (TriGuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth - InsetLeft))
            {
                base.OnGUI(contentRect);
            }
        }
    }
}