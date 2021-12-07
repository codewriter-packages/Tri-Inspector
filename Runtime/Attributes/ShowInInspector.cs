using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class ShowInInspector : Attribute
    {
    }
}