using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    public static class TriMessageTypeExtensions
    {
        public static Texture2D GetIcon(this TriMessageType messageType)
        {
            var icon = messageType switch
            {
                TriMessageType.Error => "console.erroricon",
                TriMessageType.Warning => "console.warnicon",
                TriMessageType.Info => "console.infoicon",
                _ => null,
            };

            return icon != null ? EditorGUIUtility.IconContent(icon).image as Texture2D : null;
        }
    }
}