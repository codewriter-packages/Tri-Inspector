using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(EnumToggleButtonsDrawer), TriDrawerOrder.Drawer,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class EnumToggleButtonsDrawer : TriAttributeDrawer<EnumToggleButtonsAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.FieldType.IsEnum)
            {
                return "EnumToggleButtons attribute can be used only on enums";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var buttons = new EnumToggleButtonsVisualElement(property)
            {
                style =
                {
                    marginRight = -3,
                },
            };
            return new TriAlignedLabelVisualElement<int>(property, buttons);
        }

        private sealed class EnumToggleButtonsVisualElement : ToggleButtonGroup
        {
            private readonly TriProperty _property;
            private readonly List<EnumEntry> _enumValues;
            private readonly bool _isFlags;

            public EnumToggleButtonsVisualElement(TriProperty property)
            {
                _property = property;
                _enumValues = Enum.GetNames(property.FieldType)
                    .Zip(Enum.GetValues(property.FieldType).OfType<Enum>(), (key, val) => new EnumEntry
                    {
                        name = key,
                        value = val,
                        displayName = ObjectNames.NicifyVariableName(key),
                    })
                    .ToList();
                _isFlags = property.FieldType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

                var enumFields = property.FieldType.GetFields(BindingFlags.Static | BindingFlags.Public);

                foreach (var enumValue in _enumValues)
                {
                    var enumField = Array.Find(enumFields, it => it.Name == enumValue.name);
                    var inspectorNameAttr = enumField?.GetCustomAttribute<InspectorNameAttribute>();
                    if (inspectorNameAttr != null)
                    {
                        enumValue.displayName = inspectorNameAttr.displayName;
                    }
                }

                _enumValues.Sort(new DeclarationOrderComparer(enumFields));

                isMultipleSelection = _isFlags;
                allowEmptySelection = _isFlags;

                foreach (var entry in _enumValues)
                {
                    Add(new Button
                    {
                        text = entry.displayName,
                        style =
                        {
                            flexGrow = 1,
                        },
                    });
                }

                this.BindTri(_property, StateFromEnum, EnumFromState, hideLabel: true);
            }

            private ToggleButtonGroupState StateFromEnum(Enum current)
            {
                var state = new ToggleButtonGroupState(0UL, _enumValues.Count);
                for (var i = 0; i < _enumValues.Count; i++)
                {
                    var itemValue = _enumValues[i].value;
                    state[i] = current != null && (_isFlags ? current.HasFlag(itemValue) : current.Equals(itemValue));
                }

                return state;
            }

            private Enum EnumFromState(ToggleButtonGroupState state)
            {
                if (_isFlags)
                {
                    long newValue = 0;
                    for (var i = 0; i < _enumValues.Count; i++)
                    {
                        if (state[i])
                        {
                            newValue |= Convert.ToInt64(_enumValues[i].value);
                        }
                    }

                    return (Enum) Enum.ToObject(_property.FieldType, newValue);
                }

                for (var i = 0; i < _enumValues.Count; i++)
                {
                    if (state[i])
                    {
                        return _enumValues[i].value;
                    }
                }

                // No selection — keep the current value.
                return (Enum) _property.Value;
            }

            private class EnumEntry
            {
                public string name;
                public string displayName;
                public Enum value;
            }

            private class DeclarationOrderComparer : IComparer<EnumEntry>
            {
                private readonly FieldInfo[] _fields;

                public DeclarationOrderComparer(FieldInfo[] fields)
                {
                    _fields = fields;
                }

                public int Compare(EnumEntry x, EnumEntry y)
                {
                    var orderX = Array.FindIndex(_fields, it => it.Name == x.name);
                    var orderY = Array.FindIndex(_fields, it => it.Name == y.name);
                    return orderX.CompareTo(orderY);
                }
            }
        }
    }
}