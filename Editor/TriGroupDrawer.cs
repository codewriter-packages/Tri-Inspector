using System;
using JetBrains.Annotations;
using TriInspector.VisualElements;

namespace TriInspector
{
    public abstract class TriGroupDrawer
    {
        public virtual TriPropertyCollectionVisualElement CreateVisualElementInternal(Attribute attribute)
        {
            return null;
        }
    }

    public abstract class TriGroupDrawer<TAttribute> : TriGroupDrawer
        where TAttribute : Attribute
    {
        public sealed override TriPropertyCollectionVisualElement CreateVisualElementInternal(Attribute attribute)
        {
            return CreateVisualElement((TAttribute) attribute);
        }
        [PublicAPI]
        public virtual TriPropertyCollectionVisualElement CreateVisualElement(TAttribute attribute)
        {
            return null;
        }
    }
}