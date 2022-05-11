using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class DisableIfAttribute : ConditionalDisableAttribute
    {
        public DisableIfAttribute(string condition) : base(condition)
        {
        }
    }
}