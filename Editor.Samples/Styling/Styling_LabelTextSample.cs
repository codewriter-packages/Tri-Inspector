using System;
using TriInspector;
using UnityEngine;

/// <summary>
/// Overrides the display label of a property with a custom static or dynamic string.
/// </summary>
public class Styling_LabelTextSample : ScriptableObject
{
    [LabelText("Custom Label")]
    public int val;

    [LabelText("$" + nameof(DynamicLabel))]
    public Vector3 vec;

    public string DynamicLabel => DateTime.Now.ToShortTimeString();
}