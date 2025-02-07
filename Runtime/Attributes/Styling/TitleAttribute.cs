using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class TitleAttribute : Attribute
    {
        public string Title { get; }
        public bool HorizontalLine { get; set; } = true;

        public TitleAttribute(string title)
        {
            Title = title;
        }
    }
}