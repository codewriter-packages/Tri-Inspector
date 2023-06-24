using System.Linq;
using TriInspector;
using UnityEngine;

public class Validators_RequiredSample : ScriptableObject
{
    [Required(FixAction = nameof(FixMaterial), FixActionName = "Find in Resources")]
    public Material material;

    private void FixMaterial()
    {
        material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault();
    }
}