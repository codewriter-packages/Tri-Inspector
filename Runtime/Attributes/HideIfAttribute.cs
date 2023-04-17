using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class HideIfAttribute : Attribute
    {
        public HideIfAttribute(string condition) : this(condition, true)
        {
        }

        public HideIfAttribute(string condition, object value)
        {
            Condition = condition;
            Values = new[] { value };
        }

        public HideIfAttribute(string condition, ConditionType conditionType, params object[] values)
        {
            Condition = condition;
            ConditionType = conditionType;
            Values = values;
        }

        public string Condition { get; }
        public ConditionType ConditionType { get; }
        public object[] Values { get; }

        public bool Inverse { get; protected set; }
    }
}