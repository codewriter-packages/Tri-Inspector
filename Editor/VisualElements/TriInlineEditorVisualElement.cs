using System;
using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TriInspector.VisualElements
{
    public class TriInlineEditorVisualElement : TriHeaderBoxedVisualElement
    {
        [Serializable]
        public struct Props
        {
            public InlineEditorModes mode;
            public float previewHeight;

            public bool DrawGUI => (mode & InlineEditorModes.GUIOnly) != 0;
            public bool DrawHeader => (mode & InlineEditorModes.Header) != 0;
            public bool DrawPreview => (mode & InlineEditorModes.Preview) != 0;
        }

        private readonly TriProperty _property;
        private readonly Props _props;
        private readonly VisualElement _content;

        private Editor _editor;
        private Object _editorTarget;

        public TriInlineEditorVisualElement(TriProperty property, Props props = default)
            : base(property, useFoldout: true, BuildObjectField(property))
        {
            _property = property;
            _props = props;

            _content = Content;
            _content.style.display = DisplayStyle.None;

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                _property.ValueChanged += OnValueChanged;
                SyncContent();
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                _property.ValueChanged -= OnValueChanged;
                DestroyEditor();
            });

            this.PeriodicRun(SyncContent);
        }

        protected override void OnExpandedChanged(bool expanded)
        {
            SyncContent();
        }

        private static ObjectField BuildObjectField(TriProperty property)
        {
            var field = new ObjectField
            {
                objectType = property.FieldType,
                allowSceneObjects = property.PropertyTree.TargetIsPersistent == false,
            };
            field.BindTri(property, hideLabel: true);
            return field;
        }

        private void OnValueChanged(TriProperty changed)
        {
            SyncContent();
        }

        private void SyncContent()
        {
            var value = _property.Value as Object;
            var shouldShow = _property.IsExpanded && !_property.IsValueMixed && value != null;

            if (!shouldShow)
            {
                if (_editor != null)
                {
                    DestroyEditor();
                    _content.Clear();
                }

                _content.style.display = DisplayStyle.None;
                return;
            }

            if (_editor == null || _editorTarget != value)
            {
                DestroyEditor();

                _editor = Editor.CreateEditor(value);
                _editorTarget = value;

                if (!InternalEditorUtilityProxy.GetIsInspectorExpanded(value))
                {
                    InternalEditorUtilityProxy.SetIsInspectorExpanded(value, true);
                }

                RebuildContent();
            }

            _content.style.display = DisplayStyle.Flex;
        }

        private void RebuildContent()
        {
            _content.Clear();

            if (_props.DrawHeader)
            {
                _content.Add(new IMGUIContainer(() =>
                {
                    if (_editor != null)
                    {
                        _editor.DrawHeader();
                    }
                }));
            }

            if (_props.DrawGUI)
            {
                _content.Add(new InspectorElement(_editor));
            }

            if (_props.DrawPreview)
            {
                var previewHeight = _props.previewHeight;

                // Preview GUIs have no UI Toolkit equivalent; keep them as an IMGUI
                _content.Add(new IMGUIContainer(() =>
                {
                    if (_editor == null || !_editor.HasPreviewGUI())
                    {
                        return;
                    }

                    var rect = EditorGUILayout.GetControlRect(false, previewHeight);
                    rect.width = Mathf.Max(rect.width, 10);
                    rect.height = Mathf.Max(rect.height, 10);

                    var guiEnabled = GUI.enabled;
                    GUI.enabled = true;

                    _editor.DrawPreview(rect);

                    GUI.enabled = guiEnabled;
                }));
            }
        }

        private void DestroyEditor()
        {
            if (_editor != null)
            {
                Object.DestroyImmediate(_editor);
                _editor = null;
            }

            _editorTarget = null;
        }
    }
}