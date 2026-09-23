using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class PreviewObjectAttribute : Attribute
    {
        public float Height { get; set; }

        public bool DrawDefaultField { get; set; } = true;
    }
}