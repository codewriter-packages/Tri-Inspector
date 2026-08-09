using UnityEditor;
using UnityEditor.UIElements;
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
                TrySetWidth();
            }
        }

        private void TrySetWidth()
        {
            if (_labelWidthSet)
            {
                return;
            }

            if (childCount == 0)
            {
                return;
            }

            if (TriLabelWidthContextVisualElement.ApplyWidthFromAncestorToPrefixLabel(this[0]))
            {
                _labelWidthSet = true;
            }
        }
    }
}