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
            var buttons = new EnumToggleButtonsVisualElement(property);
            return new TriAlignedLabelVisualElement(property, buttons);
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

                this.RegisterValueChangedCallback(OnValueChanged);
                this.PeriodicRun(RefreshFromProperty);
            }

            private Enum GetCurrentValue()
            {
                return _property.TryGetSerializedProperty(out var serializedProperty)
                    ? (Enum) Enum.ToObject(_property.FieldType, serializedProperty.longValue)
                    : (Enum) _property.Value;
            }

            private void RefreshFromProperty()
            {
                showMixedValue = _property.IsValueMixed;
                if (_property.IsValueMixed)
                {
                    return;
                }

                var current = GetCurrentValue();

                var state = new ToggleButtonGroupState(0UL, _enumValues.Count);
                for (var i = 0; i < _enumValues.Count; i++)
                {
                    var itemValue = _enumValues[i].value;
                    state[i] = current != null && (_isFlags ? current.HasFlag(itemValue) : current.Equals(itemValue));
                }

                SetValueWithoutNotify(state);
            }

            private void OnValueChanged(ChangeEvent<ToggleButtonGroupState> evt)
            {
                var state = evt.newValue;

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

                    _property.SetValue((Enum) Enum.ToObject(_property.FieldType, newValue));
                }
                else
                {
                    for (var i = 0; i < _enumValues.Count; i++)
                    {
                        if (state[i])
                        {
                            _property.SetValue(_enumValues[i].value);
                            break;
                        }
                    }
                }
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