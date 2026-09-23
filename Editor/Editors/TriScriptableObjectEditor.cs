using UnityEditor;
using UnityEngine;

namespace TriInspector.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), editorForChildClasses: true, isFallback = true)]
    internal sealed class TriScriptableObjectEditor : TriEditor
    {
    }
}