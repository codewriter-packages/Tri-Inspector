using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    internal static class TriStyleSheet
    {
        private static IReadOnlyList<StyleSheet> _sheets;

        private static IReadOnlyList<StyleSheet> Sheets
        {
            get
            {
                if (_sheets == null)
                {
                    _sheets = AssetDatabase.FindAssets("*.TriStyleSheet t:StyleSheet")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Select(AssetDatabase.LoadAssetAtPath<StyleSheet>)
                        .ToList();
                }

                return _sheets;
            }
        }

        public static void ApplyTo(VisualElement element)
        {
            foreach (var sheet in Sheets)
            {
                element.styleSheets.Add(sheet);
            }
        }
    }
}