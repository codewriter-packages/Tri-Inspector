using TriInspector;
using UnityEngine;

/// <summary>
/// Enables a property for editing in the inspector only while the editor is in edit mode.
/// </summary>
public class Conditionals_EnableInEditModeSample : ScriptableObject
{
    [EnableInEditMode]
    public float val;
}