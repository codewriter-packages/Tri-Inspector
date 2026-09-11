using System;
using TriInspector;
using UnityEngine;

/// <summary>
/// Attaches a static or dynamic tooltip to a property label in the inspector.
/// </summary>
public class Styling_PropertyTooltipSample : ScriptableObject
{
    [PropertyTooltip("This is tooltip")]
    public Rect rect;

    [PropertyTooltip("$" + nameof(DynamicTooltip))]
    public Vector3 vec;

    public string DynamicTooltip => DateTime.Now.ToShortTimeString();
}