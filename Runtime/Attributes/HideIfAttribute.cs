using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class HideIfAttribute : ConditionalHideAttribute
    {
        public HideIfAttribute(string condition) : base(condition)
        {
        }
    }
}