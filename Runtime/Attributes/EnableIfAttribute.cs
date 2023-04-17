using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class EnableIfAttribute : DisableIfAttribute
    {
        public EnableIfAttribute(string condition) : this(condition, true)
        {
        }
        
        public EnableIfAttribute(string condition, object value) 
            : base(condition, value)
        {
            Inverse = true;
        }
        
        public EnableIfAttribute(string condition, ConditionType conditionType, params object[] values) 
            : base(condition, conditionType, values)
        {
            Inverse = true;
        }
    }
}