using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
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
                var dropdown = new ReferenceTypeDropDown(property, new AdvancedDropdownState());
                dropdown.Show(rect);
                Event.current.Use();
            }
        }

        private class ReferenceTypeDropDown : AdvancedDropdown
        {
            private readonly TriProperty _property;

            public ReferenceTypeDropDown(TriProperty property, AdvancedDropdownState state) : base(state)
            {
                _property = property;
                minimumSize = new Vector2(0, 120);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var types = TriReflectionUtilities
                    .AllNonAbstractTypes
                    .Where(type => _property.FieldType.IsAssignableFrom(type))
                    .Where(type => type.GetConstructor(Type.EmptyTypes) != null)
                    .ToList();

                var root = new AdvancedDropdownItem("Type");
                root.AddChild(new ReferenceTypeItem(null));
                root.AddSeparator();

                foreach (var type in types)
                {
                    root.AddChild(new ReferenceTypeItem(type));
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (!(item is ReferenceTypeItem referenceTypeItem))
                {
                    return;
                }

                if (referenceTypeItem.Type == null)
                {
                    _property.SetValue(null);
                }
                else
                {
                    var instance = Activator.CreateInstance(referenceTypeItem.Type);
                    _property.SetValue(instance);
                }
            }

            private class ReferenceTypeItem : AdvancedDropdownItem
            {
                public ReferenceTypeItem(Type type) : base(type?.Name ?? "[None]")
                {
                    Type = type;
                }

                public Type Type { get; }
            }
        }
    }
}