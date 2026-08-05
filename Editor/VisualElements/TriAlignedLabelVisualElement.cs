using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriAlignedLabelVisualElement : BaseField<object>
    {
        public TriAlignedLabelVisualElement(TriProperty property, VisualElement content) : this(string.Empty, content)
        {
            this.AutoSyncLabelFromProperty(property);
        }

        public TriAlignedLabelVisualElement(string label, VisualElement content) : base(label, content)
        {
            AddToClassList(alignedFieldUssClassName);
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

            var overlay = new TriAlignedLabelVisualElement(" ", content);
            overlay.AddToClassList(TriStyles.ReferenceTypeOverlay);
            overlay.pickingMode = PickingMode.Ignore;
            toggle.Add(overlay);
        }
    }
}