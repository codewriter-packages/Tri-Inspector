using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;
using TriInspector.GroupDrawers;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: RegisterTriAttributeDrawer(typeof(InlineEditorDrawer), TriDrawerOrder.Fallback - 1000,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class InlineEditorDrawer : TriAttributeDrawer<InlineEditorAttribute>
    {
        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            if (!typeof(Object).IsAssignableFrom(property.FieldType))
            {
                var stack = new TriElement();
                stack.AddChild(new TriInfoBoxElement($"InlineEditor valid only on Object fields",
                    MessageType.Error));
                stack.AddChild(next);

                return stack;
            }

            var element = new TriBoxGroupDrawer.TriBoxGroupElement(new DeclareBoxGroupAttribute(""));
            element.AddChild(new ObjectReferenceFoldoutDrawerElement(property));
            element.AddChild(new InlineEditorDrawerElement(property));
            return element;
        }

        private class ObjectReferenceFoldoutDrawerElement : TriElement
        {
            private readonly TriProperty _property;

            public ObjectReferenceFoldoutDrawerElement(TriProperty property)
            {
                _property = property;
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                var prefixRect = new Rect(position)
                {
                    height = EditorGUIUtility.singleLineHeight,
                    xMax = position.xMin + EditorGUIUtility.labelWidth,
                };
                var pickerRect = new Rect(position)
                {
                    height = EditorGUIUtility.singleLineHeight,
                    xMin = prefixRect.xMax,
                };

                TriGuiHelper.PushIndentLevel();
                TriEditorGUI.Foldout(prefixRect, _property);
                TriGuiHelper.PopIndentLevel();

                EditorGUI.BeginChangeCheck();

                var allowSceneObjects = _property.PropertyTree.TargetObjects[0] is var targetObject &&
                                        targetObject != null && !EditorUtility.IsPersistent(targetObject);

                var value = (Object) _property.Value;
                value = EditorGUI.ObjectField(pickerRect, GUIContent.none, value,
                    _property.FieldType, allowSceneObjects);

                if (EditorGUI.EndChangeCheck())
                {
                    _property.SetValue(value);
                }
            }
        }

        private class InlineEditorDrawerElement : TriElement
        {
            private readonly TriProperty _property;
            private Editor _editor;
            private Rect _editorPosition;
            private bool _dirty;

            public InlineEditorDrawerElement(TriProperty property)
            {
                _property = property;
                _editorPosition = Rect.zero;
            }

            protected override void OnDetachFromPanel()
            {
                if (_editor != null)
                {
                    Object.DestroyImmediate(_editor);
                }

                base.OnDetachFromPanel();
            }

            public override bool Update()
            {
                if (_editor == null || _editor.target != (Object) _property.Value)
                {
                    if (_editor != null)
                    {
                        Object.DestroyImmediate(_editor);
                    }

                    _dirty = true;
                }

                if (_dirty)
                {
                    _dirty = false;
                    return true;
                }

                return false;
            }

            public override float GetHeight(float width)
            {
                if (_property.IsExpanded && !_property.IsValueMixed)
                {
                    return _editorPosition.height;
                }

                return 0f;
            }

            public override void OnGUI(Rect position)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    _editorPosition = position;
                }

                var lastEditorRect = Rect.zero;

                if (TriGuiHelper.IsEditorForObjectPushed((Object) _property.Value))
                {
                    GUI.Label(position, "Recursive inline editors not supported");

                    lastEditorRect.height = EditorGUIUtility.singleLineHeight;
                }
                else
                {
                    if (_editor == null)
                    {
                        _editor = Editor.CreateEditor((Object) _property.Value);
                    }

                    if (_editor != null && _property.IsExpanded && !_property.IsValueMixed)
                    {
                        TriGuiHelper.PushIndentLevel();
                        var indentedEditorPosition = EditorGUI.IndentedRect(_editorPosition);
                        TriGuiHelper.PopIndentLevel();

                        GUILayout.BeginArea(indentedEditorPosition);
                        GUILayout.BeginVertical();
                        _editor.OnInspectorGUI();
                        GUILayout.EndVertical();
                        lastEditorRect = GUILayoutUtility.GetLastRect();
                        GUILayout.EndArea();
                    }
                    else
                    {
                        if (_editor != null)
                        {
                            Object.DestroyImmediate(_editor);
                        }
                    }
                }

                if (Event.current.type == EventType.Repaint &&
                    !Mathf.Approximately(_editorPosition.height, lastEditorRect.height))
                {
                    _editorPosition.height = lastEditorRect.height;
                    _dirty = true;
                    _property.PropertyTree.RequestRepaint();
                }
            }
        }
    }
}