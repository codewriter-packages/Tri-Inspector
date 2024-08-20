using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class TypeConstraintAttribute : Attribute
    {
        public Type AssemblyType { get; private set; }

        public bool AllowAbstract { get; set; }

        public TypeConstraintAttribute(Type assemblyType)
        {
            AssemblyType = assemblyType;
        }
    }
}