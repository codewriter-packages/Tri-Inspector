#if TRI_MODULE_ANIMATION

using UnityEngine;
using TriInspector;

public class Decorators_AnimatorParameterSample : ScriptableObject
{
    [AnimatorParameter(nameof(animator))]
    public string parameterName;

    [AnimatorParameter(nameof(animator), AnimatorControllerParameterType.Float)]
    public int parameterHash;

    public Animator animator;
}

#endif