using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriFoldoutVisualElement : VisualElement
    {
        public TriFoldoutVisualElement(TriProperty property)
        {
            var foldout = new Foldout
            {
                value = property.IsExpanded,
            };

            foldout.SetAcceptClicksIfDisabled(true);
            foldout.AutoSyncLabelFromProperty(property);
            foldout.AddToClassList(TriStyles.Foldout);

            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                foldout.BindProperty(serializedProperty);
            }

            var built = false;

            void BuildContentIfNeeded()
            {
                if (built)
                {
                    return;
                }

                built = true;

                foldout.Add(new TriPropertyCollectionVisualElement(property.ValueType, property.ChildrenProperties));
            }

            if (property.IsExpanded)
            {
                BuildContentIfNeeded();
            }

            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.target != foldout)
                {
                    return;
                }

                property.IsExpanded = evt.newValue;

                if (evt.newValue)
                {
                    BuildContentIfNeeded();
                }
            });

            VisualElement el = foldout;
            el = new TriLabelWidthContextVisualElement(null, el);
            Add(el);
        }
    }
}