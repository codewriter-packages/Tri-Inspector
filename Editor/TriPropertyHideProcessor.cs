using System;

namespace TriInspector
{
    public abstract class TriPropertyHideProcessor
    {
        internal Attribute RawAttribute { get; set; }

        public abstract bool IsHidden(TriProperty property);
    }

    public abstract class TriPropertyHideProcessor<TAttribute> : TriPropertyHideProcessor
        where TAttribute : HideBaseAttribute
    {
    }
}