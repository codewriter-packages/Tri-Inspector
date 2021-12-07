using UnityEditor;

namespace TriInspector.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TriScriptableObject), true, isFallback = true)]
    internal sealed class TriScriptableObjectEditor : TriEditor
    {
    }
}