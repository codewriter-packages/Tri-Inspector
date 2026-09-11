using TriInspector;
using UnityEngine;

/// <summary>
/// Restricts an object reference field to scene objects only, showing an error for project assets.
/// </summary>
public class Validators_SceneObjectsOnlySample : ScriptableObject
{
    [SceneObjectsOnly]
    public GameObject obj;
}