using TriInspector;
using UnityEngine;

/// <summary>
/// Embeds the full inspector of a referenced asset inline within the parent inspector.
/// </summary>
public class Decorators_InlineEditorSample : ScriptableObject
{
    [InlineEditor]
    public Material mat;
}