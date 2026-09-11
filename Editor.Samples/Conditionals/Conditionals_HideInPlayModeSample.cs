using TriInspector;
using UnityEngine;

/// <summary>
/// Hides a property in the inspector while the editor is in play mode.
/// </summary>
public class Conditionals_HideInPlayModeSample : ScriptableObject
{
    [HideInPlayMode]
    public float val;
}