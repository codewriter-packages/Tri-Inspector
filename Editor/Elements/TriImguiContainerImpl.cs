using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    internal sealed class TriImguiContainerImpl : IMGUIContainer, ITriElement
    {
        private readonly TriProperty _property;
        private readonly TriElement _element;
        private readonly bool _applyPropertyContext;

        public TriImguiContainerImpl(TriProperty property, TriElement element, bool applyPropertyContext = false)
        {
            _property = property;
            _element = element;
            _applyPropertyContext = applyPropertyContext;

            onGUIHandler = OnElementGui;

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);

            // red color for legacy IMGUI elements so we can see which elements still not migrated
            style.borderLeftColor = new StyleColor(Color.red);
            style.borderLeftWidth = new StyleFloat(2);
            style.backgroundColor = new StyleColor(new Color(1f, 0f, 0f, 0.05f));
        }

        public VisualElement CreateVisualElement(TriProperty property)
        {
            return this;
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (!_element.IsAttached)
            {
                _element.AttachInternal();
            }
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (_element.IsAttached)
            {
                _element.DetachInternal();
            }
        }

        private void OnElementGui()
        {
            if (!_element.IsAttached)
            {
                return;
            }

            var hasSerializedProperty = _property.TryGetSerializedProperty(out var serializedProperty);
            if (hasSerializedProperty)
            {
                serializedProperty.serializedObject.UpdateIfRequiredOrScript();
            }
            
            _element.Update();

            var width = contentRect.width;

            if (width <= 0f || float.IsNaN(width))
            {
                return;
            }

            const float labelWidthRatio = 0.45f;
            const float labelMinWidth = 120f;
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.hierarchyMode = false;
            EditorGUIUtility.labelWidth = Mathf.Max(labelMinWidth, width * labelWidthRatio - 2f);

            var height = _element.GetHeight(width);
            var rect = GUILayoutUtility.GetRect(width, height);

            if (_applyPropertyContext)
            {
                DrawElementWithPropertyContext(rect, hasSerializedProperty, serializedProperty);
            }
            else
            {
                _element.OnGUI(rect);
            }

            if (hasSerializedProperty)
            {
                if (serializedProperty.serializedObject.ApplyModifiedProperties())
                {
                    _property.PropertyTree.RequestValidation();
                }
            }
        }

        private void DrawElementWithPropertyContext(Rect rect, bool hasSerializedProperty,
            SerializedProperty serializedProperty)
        {
            var oldShowMixedValue = EditorGUI.showMixedValue;
            var oldEnabled = GUI.enabled;

            GUI.enabled &= _property.IsEnabled;
            EditorGUI.showMixedValue = _property.IsValueMixed;

            if (hasSerializedProperty)
            {
                EditorGUI.BeginProperty(rect, null, serializedProperty);
            }

            _element.OnGUI(rect);

            if (hasSerializedProperty)
            {
                EditorGUI.EndProperty();
            }

            EditorGUI.showMixedValue = oldShowMixedValue;
            GUI.enabled = oldEnabled;
        }
    }
}
