using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Validator
{
    public sealed class TriAutoValidateToggleVisualElement : VisualElement
    {
        private static readonly Color ActiveColor = new Color(0.22f, 0.47f, 0.85f);

        private readonly ToolbarToggle _toggle;
        private readonly ToolbarMenu _dropdown;

        public TriAutoValidateToggleVisualElement()
        {
            style.flexDirection = FlexDirection.Row;

            _toggle = new ToolbarToggle
            {
                text = "Auto",
                tooltip = "Automatically validate",
            };
            _toggle.SetValueWithoutNotify(TriValidationPreferences.Auto);
            _toggle.RegisterValueChangedCallback(evt =>
            {
                TriValidationPreferences.Auto = evt.newValue;
                SetActiveHighlight(evt.newValue);
            });
            Add(_toggle);

            _dropdown = new ToolbarMenu
            {
                tooltip = "Choose what is auto validated",
            };

            var names = Enum.GetNames(typeof(TriValidationAutoTrigger));
            var values = (TriValidationAutoTrigger[]) Enum.GetValues(typeof(TriValidationAutoTrigger));
            for (var i = 0; i < values.Length; i++)
            {
                AppendToggleAction(GetLabel(names[i]), values[i]);
            }

            Add(_dropdown);

            SetActiveHighlight(TriValidationPreferences.Auto);
        }

        private static string GetLabel(string name)
        {
            var field = typeof(TriValidationAutoTrigger).GetField(name);
            var attribute = field.GetCustomAttribute<InspectorNameAttribute>();
            return attribute != null ? attribute.displayName : name;
        }

        private void AppendToggleAction(string actionName, TriValidationAutoTrigger trigger)
        {
            _dropdown.menu.AppendAction(actionName,
                _ => TriValidationPreferences.SetAutoTriggerEnabled(
                    trigger, !TriValidationPreferences.IsAutoTriggerEnabled(trigger)),
                _ => TriValidationPreferences.IsAutoTriggerEnabled(trigger)
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
        }

        private void SetActiveHighlight(bool active)
        {
            SetActiveHighlight(_toggle, active);
            SetActiveHighlight(_dropdown, active);
        }

        private static void SetActiveHighlight(VisualElement element, bool active)
        {
            if (active)
            {
                element.style.backgroundColor = ActiveColor;
                element.style.color = Color.white;
            }
            else
            {
                element.style.backgroundColor = StyleKeyword.Null;
                element.style.color = StyleKeyword.Null;
            }
        }
    }
}