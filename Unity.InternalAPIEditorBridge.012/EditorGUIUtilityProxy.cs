using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    internal static class EditorGUIUtilityProxy
    {
        public static Texture2D GetHelpIcon(MessageType type)
        {
            return EditorGUIUtility.GetHelpIcon(type);
        }
    }
}