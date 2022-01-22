using System;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    internal static class ScriptAttributeUtilityProxy
    {
        private static Func<SerializedProperty, object> _getHandler;

        public static PropertyHandlerProxy GetHandler(SerializedProperty property)
        {
            if (_getHandler == null)
            {
                _getHandler = TriReflectionUtilities
                    .GetUnityEditorTypeByFullName("UnityEditor.ScriptAttributeUtility")
                    .CompileStaticMethod<SerializedProperty, object>("GetHandler");
            }

            return new PropertyHandlerProxy(_getHandler(property));
        }
    }

    public readonly struct PropertyHandlerProxy
    {
        private static Func<object, bool> _hasPropertyDrawerProperty;
        private static Func<object, SerializedProperty, GUIContent, bool, float> _getHeightMethod;
        private static Func<object, Rect, SerializedProperty, GUIContent, bool, bool> _onGuiMethod;

        private readonly object _self;

        internal PropertyHandlerProxy(object self)
        {
            _self = self;
        }

        // ReSharper disable once InconsistentNaming
        public bool hasPropertyDrawer
        {
            get
            {
                if (_hasPropertyDrawerProperty == null)
                {
                    _hasPropertyDrawerProperty = TriReflectionUtilities
                        .GetUnityEditorTypeByFullName("UnityEditor.PropertyHandler")
                        .CompileInstanceProperty<bool>("hasPropertyDrawer");
                }

                return _hasPropertyDrawerProperty(_self);
            }
        }

        public float GetHeight(
            SerializedProperty property,
            GUIContent label,
            bool includeChildren)
        {
            if (_getHeightMethod == null)
            {
                _getHeightMethod = TriReflectionUtilities
                    .GetUnityEditorTypeByFullName("UnityEditor.PropertyHandler")
                    .CompileInstanceMethod<SerializedProperty, GUIContent, bool, float>("GetHeight");
            }

            return _getHeightMethod(_self, property, label, includeChildren);
        }

        public bool OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            bool includeChildren)
        {
            if (_onGuiMethod == null)
            {
                _onGuiMethod = TriReflectionUtilities
                    .GetUnityEditorTypeByFullName("UnityEditor.PropertyHandler")
                    .CompileInstanceMethod<Rect, SerializedProperty, GUIContent, bool, bool>("OnGUI");
            }

            return _onGuiMethod(_self, position, property, label, includeChildren);
        }
    }
}