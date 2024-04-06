using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                    AttributeTargets.Class | AttributeTargets.Struct)]
    [Conditional("UNITY_EDITOR")]
    public class InlineEditorAttribute : Attribute
    {
        public InlineEditorModes Mode { get; set; } = InlineEditorModes.GUIOnly;

        public float PreviewHeight { get; set; } = 50;

        public InlineEditorAttribute()
        {
        }

        public InlineEditorAttribute(InlineEditorModes mode)
        {
            Mode = mode;
        }
    }
}