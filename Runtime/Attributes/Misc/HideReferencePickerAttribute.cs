using System;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                    AttributeTargets.Class | AttributeTargets.Struct)]
    public class HideReferencePickerAttribute : Attribute
    {
    }
}