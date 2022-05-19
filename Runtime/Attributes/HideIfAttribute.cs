using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class HideIfAttribute : ConditionalHideBaseAttribute
    {
        public HideIfAttribute(string condition) : this(condition, true)
        {
        }

        public HideIfAttribute(string condition, object value) : base(condition, value)
        {
        }
    }
}