using UnityEditor;
using UnityEngine;

namespace TriInspector.Editors
{
#if TRI_INSPECTOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    internal sealed class ScriptableObjectEditor : TriEditor
    {
    }
#endif
}