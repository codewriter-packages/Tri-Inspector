using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    internal static class TriManagedReferenceGui
    {
        public static void DrawTypeSelector(Rect rect, TriProperty property)
        {
            var typeNameContent = new GUIContent(property.ValueType?.Name ?? "[None]");

            if (EditorGUI.DropdownButton(rect, typeNameContent, FocusType.Passive))
            {
                CreateSelectorMenu(property);
                Event.current.Use();
            }
        }

        private static void CreateSelectorMenu(TriProperty property)
        {
            var types = TriReflectionUtilities
                .AllNonAbstractTypes
                .Where(type => property.FieldType.IsAssignableFrom(type))
                .Where(type => type.GetConstructor(Type.EmptyTypes) != null)
                .ToList();

            var context = new GenericMenu();

            // None
            {
                var on = property.ValueType == null;
                context.AddItem(new GUIContent("[None]"), on, () => property.SetValue(null));
            }

            context.AddSeparator("");

            foreach (var itemType in types)
            {
                var type = itemType;

                var on = property.ValueType == type;
                context.AddItem(new GUIContent(type.Name), on, () =>
                {
                    var instance = Activator.CreateInstance(type);
                    property.SetValue(instance);
                });
            }

            context.ShowAsContext();
        }
    }
}