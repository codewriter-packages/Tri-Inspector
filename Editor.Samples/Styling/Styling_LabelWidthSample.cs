using TriInspector;
using UnityEngine;

/// <summary>
/// Overrides the width of a property label in the inspector.
/// </summary>
public class Styling_LabelWidthSample : ScriptableObject
{
    public int defaultWidth;

    [LabelWidth(40)]
    public int thin;

    [LabelWidth(300)]
    public int customInspectorVeryLongPropertyName;
}