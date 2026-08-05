using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriBuiltInPropertyVisualElement : PropertyField
    {
        private VisualElement _child;

        public TriBuiltInPropertyVisualElement(TriProperty property, SerializedProperty serializedProperty)
            : base(serializedProperty)
        {
            this.AutoSyncLabelFromProperty(property);
            this.BindProperty(serializedProperty);
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
            if (this.FindAncestor<TriLabelWidthContextVisualElement>() is { } labelContext &&
                labelContext.LabelWidth is { } customLabelWidth &&
                this.Q<Label>(className: BaseField<object>.labelUssClassName) is { } childLabel)
            {
                _child.RemoveFromClassList(BaseField<object>.alignedFieldUssClassName);
                childLabel.style.width = childLabel.style.minWidth = customLabelWidth;
            }
        }
    }
}