using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriAlignedLabelForGenericVisualElement : TriAlignedLabelVisualElement<object>
    {
        public Foldout Foldout { get; }

        public TriAlignedLabelForGenericVisualElement(TriProperty property, VisualElement content,
            bool collapsible = false)
            : base(string.IsNullOrEmpty(property.DisplayName) ? null : " ", content)
        {
            AddToClassList(TriStyles.TriAlignedGeneric);

            Foldout = new Foldout
            {
                toggleOnLabelClick = collapsible,
                value = !collapsible || property.IsExpanded,
            };

            if (collapsible)
            {
                Foldout.SetAcceptClicksIfDisabled(true);
            }
            else
            {
                AddToClassList(TriStyles.TriAlignedGenericNonCollapsible);
            }

            Foldout.AutoSyncLabelFromProperty(property);

            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                Foldout.BindProperty(serializedProperty);
            }

            labelElement.Add(Foldout);
        }
    }

    public class TriAlignedLabelVisualElement<T> : BaseField<T>
    {
        public TriAlignedLabelVisualElement(TriProperty property, VisualElement content)
            : this(property.DisplayName, content)
        {
            // This BaseField is only a layout wrapper around arbitrary content; its value is never
            // read or written. We bind it to the serialized property purely so Unity draws the native
            // prefab-override bar / context menu against the aligned label.
            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                this.BindProperty(serializedProperty);
            }

            this.AutoSyncLabelFromProperty(property);
        }

        protected TriAlignedLabelVisualElement(string label, VisualElement content)
            : base(label, content)
        {
            content.AddToClassList(TriStyles.TriAlignedLabelContent);

            TriLabelWidthContextVisualElement.SetupAlignedLabel(this);
        }
    }

    public class TriAlignedSpacerVisualElement : TriAlignedLabelVisualElement<object>
    {
        public TriAlignedSpacerVisualElement(VisualElement content) : base(" ", content)
        {
        }

        /// <summary>
        /// Overlays content onto a foldout's toggle: the content sits in the value column
        /// while the foldout arrow + title stay in the label column.
        /// The overlay ignores picking so the arrow underneath stays clickable.
        /// </summary>
        public static void InjectAlignedLabelFieldIntoFoldout(Foldout foldout, VisualElement content)
        {
            if (foldout.Q<Toggle>() is not { } toggle)
            {
                Debug.LogError("Failed to inject custom content into foldout");
                return;
            }

            var overlay = new TriAlignedSpacerVisualElement(content);
            overlay.AddToClassList(TriStyles.ReferenceTypeOverlay);
            overlay.pickingMode = PickingMode.Ignore;
            toggle.Add(overlay);
        }
    }
}