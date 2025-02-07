using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class DeclareToggleGroupAttribute : DeclareGroupBaseAttribute
    {
        public DeclareToggleGroupAttribute(string path) : base(path)
        {
            Title = path;
        }
        
        public string Title { get; set; }
        public bool Collapsible { get; set; } = true;
    }
}