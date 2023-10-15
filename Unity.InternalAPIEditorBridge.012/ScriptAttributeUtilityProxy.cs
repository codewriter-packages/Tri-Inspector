using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspectorUnityInternalBridge
{
    internal static class ScriptAttributeUtilityProxy
    {
        public static PropertyHandlerProxy GetHandler(SerializedProperty property)
        {
            var handler = ScriptAttributeUtility.GetHandler(property);
            return new PropertyHandlerProxy(handler);
        }
    }

    internal readonly struct PropertyHandlerProxy
    {
        private readonly PropertyHandler _handler;

        internal PropertyHandlerProxy(PropertyHandler handler)
        {
            _handler = handler;
        }

        // ReSharper disable once InconsistentNaming
        public bool hasPropertyDrawer => _handler.hasPropertyDrawer;

        public void SetPreferredLabel(string label)
        {
#if UNITY_2022_2_OR_NEWER
            if (_handler.propertyDrawer != null)
            {
                _handler.propertyDrawer.m_PreferredLabel = label;
            }
#endif
        }

        public VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return _handler.propertyDrawer?.CreatePropertyGUI(property);
        }

        public float GetHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return _handler.GetHeight(property, label, includeChildren);
        }

        public bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return _handler.OnGUI(position, property, label, includeChildren);
        }
    }
}