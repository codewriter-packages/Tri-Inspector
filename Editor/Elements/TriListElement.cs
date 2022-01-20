using System;
using System.Collections;
using TriInspector.Utilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TriInspector.Elements
{
    internal class TriListElement : TriElement
    {
        private static readonly Action<object, Rect> ReorderableListDrawHeaderMethod;
        private static readonly Action<object> ReorderableListClearCacheRecursiveMethod;

        private readonly TriProperty _property;
        private readonly ReorderableList _reorderableListGui;

        private float _lastContentWidth;

        static TriListElement()
        {
            ReorderableListDrawHeaderMethod = typeof(ReorderableList)
                .CompileVoidInstanceMethod<Rect>("DoListHeader");

            ReorderableListClearCacheRecursiveMethod = typeof(ReorderableList)
                .CompileVoidInstanceMethod("ClearCacheRecursive");
        }

        public TriListElement(TriProperty property)
        {
            _property = property;
            _reorderableListGui = new ReorderableList(null, _property.ArrayElementType)
            {
                draggable = false,
                displayAdd = true,
                displayRemove = true,
                drawHeaderCallback = DrawHeaderCallback,
                elementHeightCallback = ElementHeightCallback,
                drawElementCallback = DrawElementCallback,
            };
        }

        public override bool Update()
        {
            var dirty = false;

            if (_property.TryGetSerializedProperty(out var serializedProperty) && serializedProperty.isArray)
            {
                _reorderableListGui.serializedProperty = serializedProperty;
            }
            else
            {
                _reorderableListGui.list = (IList) _property.Value;
            }

            if (_property.IsExpanded)
            {
                dirty |= GenerateChildren();
            }
            else
            {
                dirty |= ClearChildren();
            }

            dirty |= base.Update();

            if (dirty)
            {
                ReorderableListClearCacheRecursiveMethod(_reorderableListGui);
            }

            return dirty;
        }

        public override float GetHeight(float width)
        {
            if (!_property.IsExpanded)
            {
                return _reorderableListGui.headerHeight + 4f;
            }

            _lastContentWidth = width;

            return _reorderableListGui.GetHeight();
        }

        public override void OnGUI(Rect position)
        {
            position = EditorGUI.IndentedRect(position);

            if (!_property.IsExpanded)
            {
                ReorderableListDrawHeaderMethod(_reorderableListGui, new Rect(position)
                {
                    yMax = position.yMax - 4,
                });
                return;
            }

            var labelWidthExtra = 6f + (_reorderableListGui.draggable ? 15f : 0f);

            EditorGUI.BeginChangeCheck();

            using (TriGuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth - labelWidthExtra))
            {
                _reorderableListGui.DoList(position);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (_reorderableListGui.list != null)
                {
                    _property.SetValue(_reorderableListGui.list);
                }
            }
        }

        private bool GenerateChildren()
        {
            var count = _reorderableListGui.count;

            if (ChildrenCount == count)
            {
                return false;
            }

            while (ChildrenCount < count)
            {
                var property = _property.ArrayElementProperties[ChildrenCount];
                AddChild(new TriPropertyElement(property, true));
            }

            while (ChildrenCount > count)
            {
                RemoveChildAt(ChildrenCount - 1);
            }

            return true;
        }

        private bool ClearChildren()
        {
            if (ChildrenCount == 0)
            {
                return false;
            }

            RemoveAllChildren();

            return true;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            using (TriGuiHelper.PushIndentLevel(-EditorGUI.indentLevel))
            {
                var labelRect = new Rect(rect)
                {
                    xMin = rect.xMin + 10,
                    xMax = rect.xMax,
                };

                var arraySizeRect = new Rect(rect)
                {
                    xMin = rect.xMax - 100,
                };

                TriEditorGUI.Foldout(labelRect, _property);
                GUI.Label(arraySizeRect, $"{_reorderableListGui.count} items", Styles.ItemsCount);
            }
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= ChildrenCount)
            {
                return;
            }

            GetChild(index).OnGUI(rect);
        }

        private float ElementHeightCallback(int index)
        {
            if (index >= ChildrenCount)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            return GetChild(index).GetHeight(_lastContentWidth);
        }

        private static class Styles
        {
            public static readonly GUIStyle ItemsCount;

            static Styles()
            {
                ItemsCount = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                    normal =
                    {
                        textColor = EditorGUIUtility.isProSkin
                            ? new Color(0.6f, 0.6f, 0.6f)
                            : new Color(0.3f, 0.3f, 0.3f),
                    },
                };
            }
        }
    }
}