using TriInspector;
using UnityEngine;

/// <summary>
/// Shows a property in the inspector only while the editor is in play mode.
/// </summary>
public class Conditionals_ShowInPlayModeSample : ScriptableObject
{
    [ShowInPlayMode]
    public float val;
}