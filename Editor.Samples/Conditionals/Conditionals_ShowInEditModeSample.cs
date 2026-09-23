using TriInspector;
using UnityEngine;

/// <summary>
/// Shows a property in the inspector only while the editor is in edit mode.
/// </summary>
public class Conditionals_ShowInEditModeSample : ScriptableObject
{
    [ShowInEditMode]
    public float val;
}