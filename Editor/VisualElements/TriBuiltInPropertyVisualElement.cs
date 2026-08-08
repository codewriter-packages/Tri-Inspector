using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriBuiltInPropertyVisualElement : PropertyField
    {
        private VisualElement _child;
        private bool _labelWidthSet;

        public TriBuiltInPropertyVisualElement(TriProperty property, SerializedProperty serializedProperty)
            : base(serializedProperty)
        {
            this.AutoSyncLabelFromProperty(property);
            this.BindProperty(serializedProperty);

            RegisterCallback<AttachToPanelEvent>(_ => _labelWidthSet = false);
            this.PeriodicRun(TrySetWidth);
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
            TrySetWidth();
        }

        private void TrySetWidth()
        {
            if (_labelWidthSet)
            {
                return;
            }

            if (this.FindAncestor<TriLabelWidthContextVisualElement>() is not { } labelContext)
            {
                return;
            }

            if (labelContext.LabelWidth is not { } customLabelWidth)
            {
                _labelWidthSet = true;
                return;
            }

            if (this.Q<Label>(className: BaseField<object>.labelUssClassName) is not { } childLabel)
            {
                return;
            }

            if (_child.ClassListContains(BaseField<object>.alignedFieldUssClassName))
            {
                _child.RemoveFromClassList(BaseField<object>.alignedFieldUssClassName);
                childLabel.style.width = childLabel.style.minWidth = customLabelWidth;
            }
            // else
            // {
            //     _labelWidthSet = true;
            // }
        }
    }
}