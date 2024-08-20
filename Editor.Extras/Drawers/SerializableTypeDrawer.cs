using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Types;
using TriInspector.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(SerializableTypeDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class SerializableTypeDrawer : TriValueDrawer<SerializableType>
    {
        public override float GetHeight(float width, TriValue<SerializableType> propertyValue, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }

        public override void OnGUI(Rect position, TriValue<SerializableType> propertyValue, TriElement next)
        {
            var serializableType = propertyValue.SmartValue;
            var typeName = serializableType != null && serializableType.Type != null
                ? TriTypeUtilities.GetTypeNiceName(serializableType.Type)
                : "[None]";
            var typeNameContent = new GUIContent(typeName);

            if (!propertyValue.Property.IsArrayElement && !propertyValue.Property.Parent.IsArrayElement &&
                !propertyValue.Property.TryGetAttribute<HideLabelAttribute>(out var hideLabelAttribute))
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), propertyValue.Property.DisplayNameContent);
            }

            if (EditorGUI.DropdownButton(position, typeNameContent, FocusType.Passive))
            {
                var dropdown = new SerializableTypeDropDown(propertyValue, new AdvancedDropdownState());
                
                dropdown.Show(position);

                Event.current.Use();
            }
        }
        
        private class SerializableTypeDropDown : AdvancedDropdown
        {
            private readonly TriValue<SerializableType> _propertyValue;

            public bool CanHideHeader { get; private set; }

            public SerializableTypeDropDown(TriValue<SerializableType> propertyValue, AdvancedDropdownState state) : base(state)
            {
                _propertyValue = propertyValue;
                
                minimumSize = new Vector2(0, 120);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                _propertyValue.Property.TryGetAttribute<TypeConstraintAttribute>(out var typeConstraintAttribute);
                
                var types = TriReflectionUtilities
                    .AllTypes
                    .Where(type => typeConstraintAttribute == null || typeConstraintAttribute.AssemblyType.IsAssignableFrom(type))
                    .Where(type => (typeConstraintAttribute == null && !type.IsAbstract) || typeConstraintAttribute != null &&
                        typeConstraintAttribute.AllowAbstract && type.IsAbstract)
                    .ToList();

                var groupByNamespace = types.Count > 20;

                CanHideHeader = !groupByNamespace;

                var root = new SerializableTypeGroupItem("Type");
                
                root.AddChild(new SerializableTypeItem(null));
                
                root.AddSeparator();

                foreach (var type in types)
                {
                    IEnumerable<string> namespaceEnumerator = groupByNamespace && type.Namespace != null
                        ? type.Namespace.Split('.')
                        : Array.Empty<string>();

                    root.AddTypeChild(type, namespaceEnumerator.GetEnumerator());
                }

                root.Build();

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (!(item is SerializableTypeItem referenceTypeItem))
                {
                    return;
                }

                if (referenceTypeItem.Type == null)
                {
                    _propertyValue.SetValue(null);
                }
                else
                {
                    _propertyValue.SetValue(new SerializableType(referenceTypeItem.Type));
                }
            }

            private class SerializableTypeGroupItem : AdvancedDropdownItem
            {
                private static readonly Texture2D ScriptIcon = EditorGUIUtility.FindTexture("cs Script Icon");

                private readonly List<SerializableTypeItem> _childItems = new ();

                private readonly Dictionary<string, SerializableTypeGroupItem> _childGroups = new ();

                public SerializableTypeGroupItem(string name) : base(name)
                {
                }

                public void AddTypeChild(Type type, IEnumerator<string> namespaceRemaining)
                {
                    if (!namespaceRemaining.MoveNext())
                    {
                        _childItems.Add(new SerializableTypeItem(type, ScriptIcon));
                        
                        return;
                    }

                    var namespaceName = namespaceRemaining.Current ?? "";

                    if (!_childGroups.TryGetValue(namespaceName, out var child))
                    {
                        _childGroups[namespaceName] = child = new SerializableTypeGroupItem(namespaceName);
                    }

                    child.AddTypeChild(type, namespaceRemaining);
                }

                public void Build()
                {
                    foreach (var child in _childGroups.Values.OrderBy(it => it.name))
                    {
                        AddChild(child);

                        child.Build();
                    }

                    AddSeparator();

                    foreach (var child in _childItems)
                    {
                        AddChild(child);
                    }
                }
            }

            private class SerializableTypeItem : AdvancedDropdownItem
            {
                public SerializableTypeItem(Type type, Texture2D preview = null)
                    : base(type != null ? TriTypeUtilities.GetTypeNiceName(type) : "[None]")
                {
                    Type = type;
                    icon = preview;
                }

                public Type Type { get; }
            }
        } 
    }
}