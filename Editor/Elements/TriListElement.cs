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
        private bool _scheduleRemove;

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

            if (_property.TryGetSerializedProperty(out var serializedProperty) && serializedProperty.isArray)
            {
                _reorderableListGui = new ReorderableList(serializedProperty.serializedObject, serializedProperty,
                    true, true, true, false);
            }
            else
            {
                _reorderableListGui = new ReorderableList((IList) _property.Value,
                    _property.ArrayElementType,
                    false, true, false, false);
            }

            _reorderableListGui.drawHeaderCallback = DrawHeaderCallback;
            _reorderableListGui.elementHeightCallback = ElementHeightCallback;
            _reorderableListGui.drawElementCallback = DrawElementCallback;
            _reorderableListGui.drawElementBackgroundCallback = DrawElementBackgroundCallback;
        }

        public override bool Update()
        {
            var dirty = false;

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
            if (!_property.IsExpanded)
            {
                ReorderableListDrawHeaderMethod(_reorderableListGui, new Rect(position)
                {
                    yMax = position.yMax - 4,
                });
                return;
            }

            var labelWidthExtra = 6f + (_reorderableListGui.draggable ? 15f : 0f);

            TriGuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth - labelWidthExtra);
            _reorderableListGui.DoList(position);
            TriGuiHelper.PopLabelWidth();

            if (_scheduleRemove)
            {
                _scheduleRemove = false;
                ReorderableList.defaultBehaviours.DoRemoveButton(_reorderableListGui);
                ReorderableListClearCacheRecursiveMethod(_reorderableListGui);
                GUI.changed = true;
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

        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index % 2 == 1)
            {
                TriEditorGUI.DrawBox(rect, GUI.skin.box);
            }
        }

        private void DrawHeaderCallback(Rect rect)
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

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var contentRect = new Rect(rect)
            {
                xMax = rect.xMax - 28,
            };
            var deleteRect = new Rect(rect)
            {
                xMin = rect.xMax - 22,
            };

            GetChild(index).OnGUI(contentRect);

            if (GUI.Button(deleteRect, Styles.IconToolbarMinus, Styles.RemoveItemButton))
            {
                _reorderableListGui.index = index;
                _scheduleRemove = true;
            }
        }

        private float ElementHeightCallback(int index)
        {
            return GetChild(index).GetHeight(_lastContentWidth);
        }

        private static class Styles
        {
            public static readonly GUIStyle ItemsCount;
            public static readonly GUIStyle RemoveItemButton;

            public static readonly GUIContent IconToolbarMinus =
                EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from the list");

            static Styles()
            {
                RemoveItemButton = new GUIStyle("RL FooterButton")
                {
                    fixedHeight = 0f,
                    alignment = TextAnchor.MiddleCenter,
                };
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