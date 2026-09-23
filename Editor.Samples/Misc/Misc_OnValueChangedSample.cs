using TriInspector;
using UnityEngine;

/// <summary>
/// Invokes callback on property modification.
/// </summary>
public class Misc_OnValueChangedSample : ScriptableObject
{
    [OnValueChanged(nameof(OnMaterialChanged))]
    public Material mat;

    private void OnMaterialChanged()
    {
        Debug.Log("Material changed!");
    }
}