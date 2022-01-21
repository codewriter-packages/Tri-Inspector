using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class InlineEditorElement : TriElement
    {
        private readonly TriProperty _property;
        private Editor _editor;
        private Rect _editorPosition;
        private bool _dirty;

        public InlineEditorElement(TriProperty property)
        {
            _property = property;
            _editorPosition = Rect.zero;
        }

        protected override void OnAttachToPanel()
        {
            base.OnAttachToPanel();

            var target = _property.Value as Object;
            if (target != null && !InternalEditorUtilityProxy.GetIsInspectorExpanded(target))
            {
                InternalEditorUtilityProxy.SetIsInspectorExpanded(target, true);
            }
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

            if (TriEditor.IsEditorForObjectPushed((Object) _property.Value))
            {
                GUI.Label(position, "Recursive inline editors not supported");

                lastEditorRect.height = EditorGUIUtility.singleLineHeight;
            }
            else
            {
                if (_editor == null && _property.IsExpanded && !_property.IsValueMixed)
                {
                    _editor = Editor.CreateEditor((Object) _property.Value);
                }

                if (_editor != null && _property.IsExpanded && !_property.IsValueMixed)
                {
                    Rect indentedEditorPosition;
                    using (TriGuiHelper.PushIndentLevel())
                    {
                        indentedEditorPosition = EditorGUI.IndentedRect(_editorPosition);
                    }

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