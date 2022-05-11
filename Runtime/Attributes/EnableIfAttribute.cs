using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class EnableIfAttribute : ConditionalDisableAttribute
    {
        public EnableIfAttribute(string condition) : base(condition)
        {
        }
    }
}