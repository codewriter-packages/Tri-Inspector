using System;

namespace TriInspector
{
    [Flags]
    public enum InlineEditorModes
    {
        GUIOnly = 1 << 0,
        Header = 1 << 1,
        Preview = 1 << 2,
        
        CompletelyHideObjectField = 1 << 3,
        DrawWithTriInspectorWithoutMonoScriptField = 1 << 4,

        GUIAndPreview = GUIOnly | Preview,
        GUIAndHeader = GUIOnly | Header,
        FullEditor = GUIOnly | Header | Preview,
    }
}