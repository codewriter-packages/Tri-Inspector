using TriInspector;
using UnityEngine;

/// <summary>
/// Enables a property for editing in the inspector only while the editor is in play mode.
/// </summary>
public class Conditionals_EnableInPlayModeSample : ScriptableObject
{
    [EnableInPlayMode]
    public float val;
}