using System;
using UnityEditor.UIElements;

namespace TriInspector.Validator
{
    public sealed class TriValidationToolbarVisualElement : Toolbar
    {
        public TriValidationToolbarVisualElement(Action onValidateScenes, Action onValidateProject, Action onClear)
        {
            AddToClassList(TriValidationStyles.Toolbar);

            Add(new TriAutoValidateToggleVisualElement());

            Add(new ToolbarSpacer());

            var scenesButton = new ToolbarButton(onValidateScenes)
            {
                text = "▶ Opened Scenes",
                tooltip = "Validate opened scenes",
            };
            Add(scenesButton);

            var projectButton = new ToolbarButton(onValidateProject)
            {
                text = "▶ Project",
                tooltip = "Validate all project assets and scenes",
            };
            Add(projectButton);

            Add(new ToolbarSpacer());

            var clearButton = new ToolbarButton(onClear)
            {
                text = "🗑",
                tooltip = "Clear validation results",
            };
            Add(clearButton);
        }
    }
}