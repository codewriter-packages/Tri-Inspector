using System;
using JetBrains.Annotations;

namespace TriInspector
{
    public abstract class TriPropertyHideProcessor
    {
        internal Attribute RawAttribute { get; set; }

        [PublicAPI]
        public virtual void Initialize(TriPropertyDefinition propertyDefinition)
        {
        }

        [PublicAPI]
        public abstract bool IsHidden(TriProperty property);
    }

    public abstract class TriPropertyHideProcessor<TAttribute> : TriPropertyHideProcessor
        where TAttribute : HideBaseAttribute
    {
        [PublicAPI]
        public TAttribute Attribute => (TAttribute) RawAttribute;
    }
}