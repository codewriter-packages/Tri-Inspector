using System;

namespace TriInspector
{
    [Flags]
    public enum InlineEditorModes
    {
        GUIOnly = 1 << 0,
        Header = 1 << 1,
        Preview = 1 << 2,

        GUIAndPreview = GUIOnly | Preview,
        GUIAndHeader = GUIOnly | Header,
        FullEditor = GUIOnly | Header | Preview,
    }
}