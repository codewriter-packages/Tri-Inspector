using TriInspector;
using UnityEngine;

/// <summary>
/// Disables a property from editing in the inspector when a specified condition is true.
/// </summary>
public class Conditionals_DisableIfSample : ScriptableObject
{
    public bool visible;

    [DisableIf(nameof(visible))]
    public float val;
}