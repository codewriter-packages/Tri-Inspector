using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class InlineEditorElement : TriElement
    {
        private readonly TriProperty _property;
        private readonly Props _props;
        private Editor _editor;
        private Rect _editorPosition;
        private bool _dirty;

        [System.Serializable]
        public struct Props
        {
            public InlineEditorModes mode;
            public float previewHeight;

            public bool DrawGUI => (mode & InlineEditorModes.GUIOnly) != 0;
            public bool DrawHeader => (mode & InlineEditorModes.Header) != 0;
            public bool DrawPreview => (mode & InlineEditorModes.Preview) != 0;
        }

        public InlineEditorElement(TriProperty property, Props props = default)
        {
            _property = property;
            _props = props;
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
            var shouldDrawEditor = _property.IsExpanded && !_property.IsValueMixed;

            if (_editor == null && shouldDrawEditor && _property.Value is Object obj && obj != null)
            {
                _editor = Editor.CreateEditor(obj);

                if (!InternalEditorUtilityProxy.GetIsInspectorExpanded(obj))
                {
                    InternalEditorUtilityProxy.SetIsInspectorExpanded(obj, true);
                }
            }

            if (_editor != null && shouldDrawEditor)
            {
                GUILayout.BeginArea(_editorPosition);
                GUILayout.BeginVertical();

                if (_props.DrawHeader || _props.DrawGUI)
                {
                    GUILayout.BeginVertical();

                    if (_props.DrawHeader)
                    {
                        GUILayout.BeginVertical();
                        _editor.DrawHeader();
                        GUILayout.EndVertical();
                    }

                    if (_props.DrawGUI)
                    {
                        GUILayout.BeginVertical();
                        _editor.OnInspectorGUI();
                        GUILayout.EndVertical();
                    }

                    GUILayout.EndVertical();
                }

                if (_props.DrawPreview && _editor.HasPreviewGUI())
                {
                    GUILayout.BeginVertical();

                    var previewOpts = new[] {GUILayout.ExpandWidth(true), GUILayout.Height(_props.previewHeight),};
                    var previewRect = EditorGUILayout.GetControlRect(false, _props.previewHeight, previewOpts);

                    previewRect.width = Mathf.Max(previewRect.width, 10);
                    previewRect.height = Mathf.Max(previewRect.height, 10);

                    var guiEnabled = GUI.enabled;
                    GUI.enabled = true;

                    _editor.DrawPreview(previewRect);

                    GUI.enabled = guiEnabled;

                    GUILayout.EndVertical();
                }

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