using UnityEditor;
using UnityEngine;

namespace TriInspector.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    internal sealed class ScriptableObjectEditor : TriEditor
    {
    }
}