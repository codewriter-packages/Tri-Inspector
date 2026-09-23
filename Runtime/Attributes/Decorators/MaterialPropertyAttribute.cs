using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MaterialPropertyAttribute : Attribute
    {
        public string MaterialFieldName { get; }
        public ShaderPropertyType? PropertyType { get; }

        public MaterialPropertyAttribute(string materialFieldName)
        {
            MaterialFieldName = materialFieldName;
        }
        public MaterialPropertyAttribute(string animatorFieldName, ShaderPropertyType parameterType) : this(animatorFieldName)
        {
            PropertyType = parameterType;
        }
    }
}