using TriInspector;
using UnityEngine;

/// <summary>
/// Disables a property from editing in the inspector while the editor is in play mode.
/// </summary>
public class Conditionals_DisableInPlayModeSample : ScriptableObject
{
    [DisableInPlayMode]
    public float val;
}