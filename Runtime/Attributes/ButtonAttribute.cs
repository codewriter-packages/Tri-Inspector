using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ButtonAttribute : Attribute
    {
        public ButtonAttribute(string name = null)
        {
            Name = name;
        }

        public string Name { get; }
    }
}