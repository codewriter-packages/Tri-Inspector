using System;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriBoxGroupElement : TriHeaderGroupBaseElement
    {
        private readonly Props _props;
        private readonly GUIContent _headerLabel;
        private bool _expanded;

        [Serializable]
        public struct Props
        {
            public bool foldout;
            public bool expandedByDefault;
        }

        public TriBoxGroupElement(string title, Props props = default)
        {
            _props = props;
            _headerLabel = new GUIContent(title ?? "");
            _expanded = _props.expandedByDefault;
        }

        protected override float GetHeaderHeight(float width)
        {
            if (!_props.foldout && string.IsNullOrEmpty(_headerLabel.text))
            {
                return 0f;
            }

            return base.GetHeaderHeight(width);
        }

        protected override float GetContentHeight(float width)
        {
            if (_props.foldout && !_expanded)
            {
                return 0f;
            }

            return base.GetContentHeight(width);
        }

        protected override void DrawHeader(Rect position)
        {
            TriEditorGUI.DrawBox(position, TriEditorStyles.TabOnlyOne);

            var headerLabelRect = new Rect(position)
            {
                xMin = position.xMin + 6,
                xMax = position.xMax - 6,
                yMin = position.yMin + 2,
                yMax = position.yMax - 2,
            };

            if (_props.foldout)
            {
                headerLabelRect.x += 10;
                _expanded = EditorGUI.Foldout(headerLabelRect, _expanded, _headerLabel, true);
            }
            else
            {
                EditorGUI.LabelField(headerLabelRect, _headerLabel);
            }
        }

        protected override void DrawContent(Rect position)
        {
            if (_props.foldout && !_expanded)
            {
                return;
            }

            base.DrawContent(position);
        }
    }
}