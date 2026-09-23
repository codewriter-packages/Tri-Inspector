using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TriInspector.Utilities
{
    public class TriUnityInspectorUtilities
    {
        private static readonly Dictionary<int, string> StandardArrayElementNames = new Dictionary<int, string>();
        
        private static readonly FieldInfo GUIStyleNameBackingField = typeof(GUIStyle)
            .GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool MustDrawWithUnity(TriProperty property)
        {
            if (property.FieldType == typeof(GUIStyle) ||
                property.FieldType == typeof(RectOffset))
            {
                return true;
            }

            return !property.IsArray && property.TryGetAttribute(out DrawWithUnityAttribute _);
        }

        public static string GetStandardArrayElementName(TriProperty property)
        {
            var index = property.IndexInArray;

            if (!StandardArrayElementNames.TryGetValue(index, out var name))
            {
                StandardArrayElementNames[index] = name = $"Element {index}";
            }

            return name;
        }

        public static bool TryGetSpecialArrayElementName(TriProperty property, out string name)
        {
            if (property.FieldType == typeof(GUIStyle) && property.Value is GUIStyle guiStyle)
            {
                GUIStyleNameBackingField?.SetValue(guiStyle, null);
                name = guiStyle.name;
                return true;
            }

            if (property.PropertyType == TriPropertyType.Generic &&
                property.ChildrenProperties.Count > 0 &&
                property.ChildrenProperties[0] is var firstChild &&
                firstChild.ValueType == typeof(string) &&
                firstChild.Value is string firstChildValueStr &&
                !string.IsNullOrEmpty(firstChildValueStr))
            {
                name = firstChildValueStr;
                return true;
            }

            name = default;
            return false;
        }
    }
}