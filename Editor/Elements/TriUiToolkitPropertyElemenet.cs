using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    internal class TriUiToolkitPropertyElement : TriElement
    {
        private readonly SerializedProperty _serializedProperty;

        private readonly VisualElement _rootElement;
        private readonly VisualElement _selfElement;

        private bool _heightDirty;

        public TriUiToolkitPropertyElement(
            TriProperty property,
            SerializedProperty serializedProperty,
            VisualElement selfElement,
            VisualElement rootElement)
        {
            _serializedProperty = serializedProperty;
            _selfElement = selfElement;
            _rootElement = rootElement;

            _selfElement.style.position = Position.Absolute;
        }

        protected override void OnAttachToPanel()
        {
            base.OnAttachToPanel();

            _rootElement.schedule.Execute(() =>
            {
                _rootElement.Add(_selfElement);
                _selfElement.Bind(_serializedProperty.serializedObject);
            });
        }

        protected override void OnDetachFromPanel()
        {
            _rootElement.schedule.Execute(() =>
            {
                _selfElement.Unbind();
                _rootElement.Remove(_selfElement);
            });

            base.OnDetachFromPanel();
        }

        public override bool Update()
        {
            var dirty = base.Update();

            if (_heightDirty)
            {
                _heightDirty = false;
                dirty = true;
            }

            return dirty;
        }

        public override float GetHeight(float width)
        {
            var height = _selfElement.resolvedStyle.height;

            if (float.IsNaN(height))
            {
                _heightDirty = true;
                return 0f;
            }

            return height;
        }

        public override void OnGUI(Rect position)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var pos = GUIClipProxy.UnClip(position.position);

                _selfElement.style.width = position.width;
                _selfElement.style.left = pos.x;
                _selfElement.style.top = pos.y;
            }
        }
    }
}