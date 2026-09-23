using TriInspector;
using UnityEngine;

/// <summary>
/// Hides a property in the inspector when a specified condition is true.
/// </summary>
public class Conditionals_HideIfSample : ScriptableObject
{
    public bool visible;

    [HideIf(nameof(visible))]
    public float val;
}