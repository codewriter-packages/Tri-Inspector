using System;

namespace TriInspector
{
    public abstract class TriPropertyDisableProcessor
    {
        internal Attribute RawAttribute { get; set; }

        public abstract bool IsDisabled(TriProperty property);
    }

    public abstract class TriPropertyDisableProcessor<TAttribute> : TriPropertyDisableProcessor
        where TAttribute : DisableBaseAttribute
    {
    }
}