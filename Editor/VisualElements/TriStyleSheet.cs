using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    internal static class TriStyleSheet
    {
        private const string ThemeDark = "tri-dark";
        private const string ThemeLight = "tri-light";

        private static TriArray<StyleSheet>? _sheets;

        private static TriArray<StyleSheet> Sheets =>
            _sheets ??= AssetDatabase.FindAssets("*.TriStyleSheet t:StyleSheet")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<StyleSheet>)
                .ToList();

        public static void ApplyTo(VisualElement element)
        {
            foreach (var sheet in Sheets)
            {
                element.styleSheets.Add(sheet);
            }

            element.AddToClassList(EditorGUIUtility.isProSkin ? ThemeDark : ThemeLight);
        }
    }
}