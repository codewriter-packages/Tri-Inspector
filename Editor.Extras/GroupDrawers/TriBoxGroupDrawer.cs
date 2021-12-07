using TriInspector;
using TriInspector.Elements;
using TriInspector.GroupDrawers;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriGroupDrawer(typeof(TriBoxGroupDrawer))]

namespace TriInspector.GroupDrawers
{
    public class TriBoxGroupDrawer : TriGroupDrawer<DeclareBoxGroupAttribute>
    {
        public override TriPropertyCollectionBaseElement CreateElement(DeclareBoxGroupAttribute attribute)
        {
            return new TriBoxGroupElement(attribute);
        }

        private class TriBoxGroupElement : TriPropertyCollectionBaseElement
        {
            private const float HeaderWidth = 22;
            private const float InsetTop = 4;
            private const float InsetBottom = 4;
            private const float InsetLeft = 4;
            private const float InsetRight = 4;

            private readonly GUIContent _headerLabel;

            public TriBoxGroupElement(DeclareBoxGroupAttribute attribute)
            {
                _headerLabel = attribute.Title == null
                    ? GUIContent.none
                    : new GUIContent(attribute.Title);
            }

            public override float GetHeight(float width)
            {
                var height = base.GetHeight(width) + InsetTop + InsetBottom;

                if (_headerLabel != GUIContent.none)
                {
                    height += HeaderWidth;
                }

                return height;
            }

            public override void OnGUI(Rect position)
            {
                var headerBgRect = new Rect(position)
                {
                    height = _headerLabel != GUIContent.none ? HeaderWidth : 0,
                };
                var headerLabelRect = new Rect(headerBgRect)
                {
                    xMin = headerBgRect.xMin + 6,
                    xMax = headerBgRect.xMax - 6,
                    yMin = headerBgRect.yMin + 2,
                    yMax = headerBgRect.yMax - 2,
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

                if (_headerLabel != GUIContent.none)
                {
                    TriEditorGUI.DrawBox(headerBgRect, TriEditorStyles.HeaderBox);
                    EditorGUI.LabelField(headerLabelRect, _headerLabel);
                    TriEditorGUI.DrawBox(contentBgRect, TriEditorStyles.ContentBox);
                }
                else
                {
                    TriEditorGUI.DrawBox(contentBgRect, TriEditorStyles.Box);
                }

                TriGuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth - InsetLeft);
                base.OnGUI(contentRect);
                TriGuiHelper.PopLabelWidth();
            }
        }
    }
}