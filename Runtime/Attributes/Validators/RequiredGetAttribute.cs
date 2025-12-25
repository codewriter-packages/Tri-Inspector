using System;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RequiredGetAttribute : Attribute
    {
        public bool InParents { get; set; }
        public bool InChildren { get; set; }
        public bool IncludeSelf { get; set; } = true;
    }
}