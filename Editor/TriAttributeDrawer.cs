using System;
using JetBrains.Annotations;

namespace TriInspector
{
    public abstract class TriAttributeDrawer : TriCustomDrawer
    {
        internal Attribute RawAttribute { get; set; }
    }

    public abstract class TriAttributeDrawer<TAttribute> : TriAttributeDrawer
        where TAttribute : Attribute
    {
        [PublicAPI]
        public TAttribute Attribute => (TAttribute) RawAttribute;
    }
}