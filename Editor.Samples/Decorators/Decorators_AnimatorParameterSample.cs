#if TRI_MODULE_ANIMATION

using UnityEngine;
using TriInspector;

/// <summary>
/// AnimatorParameter automatically lists all available parameters from the target Animator,
/// with optional filtering by parameter type.
/// </summary>
public class Decorators_AnimatorParameterSample : ScriptableObject
{
    [AnimatorParameter(nameof(animator))]
    public string parameterName;

    [AnimatorParameter(nameof(animator), AnimatorControllerParameterType.Float)]
    public int parameterHash;

    public Animator animator;
}

#endif