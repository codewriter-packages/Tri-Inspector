using TriInspector;
using UnityEngine;

/// <summary>
/// Enables a property for editing in the inspector when a specified condition is true.
/// </summary>
public class Conditionals_EnableIfSample : ScriptableObject
{
    public bool visible;

    [EnableIf(nameof(visible))]
    public float val;
}