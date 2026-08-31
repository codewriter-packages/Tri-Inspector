using UnityEditor.UIElements;
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
                AddToClassList(TriStyles.TriAlignedGenericCollapsible);
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
}