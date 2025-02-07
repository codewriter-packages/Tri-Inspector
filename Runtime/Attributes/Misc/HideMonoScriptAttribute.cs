using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Struct))]
    [Conditional("UNITY_EDITOR")]
    public class HideMonoScriptAttribute : Attribute
    {
    }
}