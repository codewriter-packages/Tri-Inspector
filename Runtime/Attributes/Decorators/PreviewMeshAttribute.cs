using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class PreviewMeshAttribute : Attribute
    {
        public int Height { get; set; } = 200;
        public int Width { get; set; } = -1;
        public bool UseFoldout { get; set; } = true;
        public PreviewMeshRotationMethod RotationMethod { get; set; } = PreviewMeshRotationMethod.Clamped;
        public PreviewMeshAttribute() { }
        public PreviewMeshAttribute(int height)
        {
            Height = height;
        }
        public PreviewMeshAttribute(int height, int width)
        {
            Height = height;
            Width = width;
        }
        public PreviewMeshAttribute(int height, int width, bool useFoldout)
        {
            Height = height;
            Width = width;
            UseFoldout = useFoldout;
        }
        public PreviewMeshAttribute(int height, int width, bool useFoldout, PreviewMeshRotationMethod rotationMethod)
        {
            Height = height;
            Width = width;
            UseFoldout = useFoldout;
            RotationMethod = rotationMethod;
        }
    }
}