using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriHeaderBoxedVisualElement : VisualElement
    {
        private readonly VisualElement _content = new VisualElement();
        private bool _expanded;

        public TriHeaderBoxedVisualElement(TriProperty property, bool useFoldout, VisualElement headerControl = null)
        {
            _expanded = !useFoldout || property.IsExpanded;

            AddToClassList(TriStyles.BoxGroup);

            if (useFoldout)
            {
                var foldout = new Foldout
                {
                    value = property.IsExpanded,
                };

                foldout.AutoSyncLabelFromProperty(property);
                foldout.AddToClassList(TriStyles.Foldout);

                if (headerControl != null)
                {
                    TriAlignedLabelVisualElement.InjectAlignedLabelFieldIntoFoldout(foldout, headerControl);
                }

                foldout.RegisterValueChangedCallback(evt =>
                {
                    // Foldout also bubbles ChangeEvent<bool> from child toggles; only react to its own.
                    if (evt.target != foldout)
                    {
                        return;
                    }

                    _expanded = evt.newValue;
                    property.IsExpanded = evt.newValue;
                    OnExpandedChanged(evt.newValue);
                });

                Add(foldout);
            }
            else if (headerControl != null)
            {
                Add(headerControl);
            }

            Add(_content);
        }

        public bool Expanded => _expanded;

        public VisualElement Content => _content;

        protected virtual void OnExpandedChanged(bool expanded)
        {
        }
    }
}