using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriPropertyField : PropertyField
    {
        private VisualElement _child;

        public TriPropertyField(SerializedProperty property, string label) : base(property, label)
        {
        }

        protected override void HandleEventBubbleUp(EventBase evt)
        {
            base.HandleEventBubbleUp(evt);

            var childChanged = childCount > 0 && _child != this[0];
            if (childChanged)
            {
                _child = this[0];
                OnChildChanged();
            }
        }

        private void OnChildChanged()
        {
            if (this.FindAncestor<TriLabelWidthContextElement>() is { } labelContext &&
                this.Q<Label>(className: BaseField<object>.labelUssClassName) is { } childLabel)
            {
                _child.RemoveFromClassList(BaseField<object>.alignedFieldUssClassName);
                childLabel.style.width = childLabel.style.minWidth = labelContext.LabelWidth;
            }
        }
    }
}