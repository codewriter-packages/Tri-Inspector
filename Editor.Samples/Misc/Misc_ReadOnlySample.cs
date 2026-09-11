using TriInspector;
using UnityEngine;

/// <summary>
/// Makes property non-editable in the inspector.
/// </summary>
public class Misc_ReadOnlySample : ScriptableObject
{
    [ReadOnly]
    public Vector3 vec;
}