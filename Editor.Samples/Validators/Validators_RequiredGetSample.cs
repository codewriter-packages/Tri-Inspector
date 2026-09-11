using TriInspector;
using UnityEngine;

/// <summary>
/// Marks a component reference field as required and automatically fetches it via GetComponent
/// when not assigned, with options to search in parents or children.
/// Note: this is a code sample only — [RequiredGet] must be used on a MonoBehaviour to work correctly.
/// </summary>
public class Validators_RequiredGetSample : ScriptableObject
{
    [RequiredGet]
    public Rigidbody rb;

    [RequiredGet(InParents = true)]
    public Animator animator;

    [RequiredGet(InChildren = true, IncludeSelf = false)]
    public MeshRenderer[] childMeshes;
}
