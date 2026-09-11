using TriInspector;
using UnityEngine;

/// <summary>
/// Renders a string field as a multi-line text area in the inspector.
/// </summary>
public class Styling_TextAreaSample : ScriptableObject
{
    public string simpleText;

    [TextArea(10, 15)]
    public string textArea;

    [PropertyTextArea]
    public string PropertyTextArea { get; set; }
}