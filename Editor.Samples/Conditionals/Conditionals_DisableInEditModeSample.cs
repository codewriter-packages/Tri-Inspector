using TriInspector;
using UnityEngine;

/// <summary>
/// Disables a property from editing in the inspector while the editor is in edit mode.
/// </summary>
public class Conditionals_DisableInEditModeSample : ScriptableObject
{
    [DisableInEditMode]
    public float val;
}