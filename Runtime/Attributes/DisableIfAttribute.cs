using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class DisableIfAttribute : ConditionalDisableBaseAttribute
    {
        public DisableIfAttribute(string condition) : this(condition, true)
        {
        }

        public DisableIfAttribute(string condition, object value) : base(condition, value)
        {
        }
    }
}