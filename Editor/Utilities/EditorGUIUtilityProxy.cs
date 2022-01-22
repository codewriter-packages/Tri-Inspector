using System;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    public class EditorGUIUtilityProxy
    {
        private static Func<MessageType, Texture2D> _getHelpIcon;

        public static Texture2D GetHelpIcon(MessageType type)
        {
            if (_getHelpIcon == null)
            {
                _getHelpIcon = TriReflectionUtilities
                    .GetUnityEditorTypeByFullName("UnityEditor.EditorGUIUtility")
                    .CompileStaticMethod<MessageType, Texture2D>("GetHelpIcon");
            }

            return _getHelpIcon(type);
        }
    }
}