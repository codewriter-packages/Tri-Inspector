using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class LabelTextAttribute : Attribute
    {
        public string Text { get; }

        public LabelTextAttribute(string text)
        {
            Text = text;
        }
    }
}