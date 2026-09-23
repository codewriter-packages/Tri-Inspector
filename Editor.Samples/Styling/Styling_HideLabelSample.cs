using TriInspector;
using UnityEngine;

/// <summary>
/// Hides the label of a property, allowing the field to expand to full width in the inspector.
/// </summary>
public class Styling_HideLabelSample : ScriptableObject
{
    [Title("Wide Vector")]
    [HideLabel]
    public Vector3 vector;

    [Title("Wide String")]
    [HideLabel]
    public string str;
}