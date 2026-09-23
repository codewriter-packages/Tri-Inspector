using TriInspector;
using UnityEngine;

/// <summary>
/// Restricts an object reference field to project assets only, showing an error for scene objects.
/// </summary>
public class Validators_AssetsOnlySample : ScriptableObject
{
    [AssetsOnly]
    public GameObject obj;
}