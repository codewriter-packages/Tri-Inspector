using System.Linq;
using TriInspector;
using UnityEngine;

/// <summary>
/// Marks a field as required and shows an error in the inspector when it is not assigned.
/// </summary>
public class Validators_RequiredSample : ScriptableObject
{
    [Required(FixAction = nameof(FixMaterial), FixActionName = "Find in Resources")]
    public Material material;

    private void FixMaterial()
    {
        material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault();
    }
}