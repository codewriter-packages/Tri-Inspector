using TriInspector;
using UnityEngine;

/// <summary>
/// Hides a property in the inspector while the editor is in edit mode.
/// </summary>
public class Conditionals_HideInEditModeSample : ScriptableObject
{
    [HideInEditMode]
    public float val;
}