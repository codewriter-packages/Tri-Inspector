using UnityEngine.UIElements;

namespace TriInspector
{
    public abstract class TriValueDrawer : TriCustomDrawer
    {
    }

    public abstract class TriValueDrawer<TValue> : TriValueDrawer
    {
        public sealed override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return CreateVisualElement(new TriValue<TValue>(property), next);
        }

        public virtual VisualElement CreateVisualElement(TriValue<TValue> propertyValue, VisualElement next)
        {
            return null;
        }
    }
}