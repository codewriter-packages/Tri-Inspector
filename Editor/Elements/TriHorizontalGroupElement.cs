using System;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriHorizontalGroupElement : TriPropertyCollectionBaseElement
    {
        private readonly float[] _sizes;
        private readonly float _totalFixedSize;

        public TriHorizontalGroupElement(float[] sizes = null)
        {
            _sizes = sizes ?? Array.Empty<float>();
            _totalFixedSize = 0f;

            for (var index = 0; index < _sizes.Length; index++)
            {
                if (TryGetFixedSizeByIndex(index, out var fixedSize))
                {
                    _totalFixedSize += fixedSize;
                }
            }
        }

        public override float GetHeight(float width)
        {
            if (ChildrenCount == 0)
            {
                return 0f;
            }

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var totalSpacing = spacing * (ChildrenCount - 1);
            var totalDynamic = width - totalSpacing - _totalFixedSize;
            var dynamicChildCount = GetDynamicChildCount();

            var height = 0f;

            for (var i = 0; i < ChildrenCount; i++)
            {
                var childWidth = GetChildWidth(i, totalDynamic, dynamicChildCount);
                var child = GetChild(i);
                var childHeight = child.GetHeight(childWidth);

                height = Mathf.Max(height, childHeight);
            }

            return height;
        }

        public override void OnGUI(Rect position)
        {
            if (ChildrenCount == 0)
            {
                return;
            }

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var totalSpacing = spacing * (ChildrenCount - 1);
            var totalDynamic = position.width - totalSpacing - _totalFixedSize;
            var dynamicChildCount = GetDynamicChildCount();

            var xOffset = 0f;
            for (var i = 0; i < ChildrenCount; i++)
            {
                var childWidth = GetChildWidth(i, totalDynamic, dynamicChildCount);
                var child = GetChild(i);
                var childRect = new Rect(position)
                {
                    width = childWidth,
                    height = child.GetHeight(childWidth),
                    x = position.xMin + xOffset,
                };

                using (TriGuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth / ChildrenCount))
                {
                    child.OnGUI(childRect);
                }

                xOffset += childWidth + spacing;
            }
        }

        private float GetDynamicChildCount()
        {
            var count = 0f;

            for (var i = 0; i < ChildrenCount; i++)
            {
                if (TryGetFixedSizeByIndex(i, out _))
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private float GetChildWidth(int i, float totalDynamic, float dynamicChildCount)
        {
            if (TryGetFixedSizeByIndex(i, out var fixedSize))
            {
                return fixedSize;
            }

            return totalDynamic / dynamicChildCount;
        }

        private bool TryGetFixedSizeByIndex(int index, out float fixedSize)
        {
            if (index < _sizes.Length && _sizes[index] > 0f)
            {
                fixedSize = _sizes[index];
                return true;
            }

            fixedSize = 0f;
            return false;
        }
    }
}