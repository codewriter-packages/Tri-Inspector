using JetBrains.Annotations;

namespace TriInspector
{
    public abstract class TriCustomDrawer
    {
        internal int Order { get; set; }
        internal bool? ApplyOnArrayElement { get; set; }

        [PublicAPI]
        public virtual void Initialize(TriPropertyDefinition propertyDefinition)
        {
        }

        [PublicAPI]
        public virtual string CanDraw(TriProperty property)
        {
            return null;
        }

        public abstract TriElement CreateElementInternal(TriProperty property, TriElement next);
    }
}