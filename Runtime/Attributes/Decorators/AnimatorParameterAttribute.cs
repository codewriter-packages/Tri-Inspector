#if TRI_MODULE_ANIMATION

using System;
using System.Diagnostics;
using UnityEngine;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class AnimatorParameterAttribute : Attribute
    {
        public string AnimatorFieldName { get; }
        public AnimatorControllerParameterType? ParameterType { get; }

        public AnimatorParameterAttribute(string animatorFieldName)
        {
            AnimatorFieldName = animatorFieldName;
        }
        public AnimatorParameterAttribute(string animatorFieldName, AnimatorControllerParameterType parameterType) : this(animatorFieldName)
        {
            ParameterType = parameterType;
        }
    }
}

#endif