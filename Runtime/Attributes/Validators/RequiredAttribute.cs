using System;

namespace TriInspector
{
    [AttributeUsage((AttributeTargets.Field | AttributeTargets.Property))]
    public class RequiredAttribute : Attribute
    {
        public string Message { get; set; }

        public string FixAction { get; set; }
        public string FixActionName { get; set; }
    }
}