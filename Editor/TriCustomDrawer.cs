using UnityEngine.UIElements;

namespace TriInspector
{
    public abstract class TriCustomDrawer : TriPropertyExtension
    {
        internal int Order { get; set; }

        public virtual VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return null;
        }
    }
}