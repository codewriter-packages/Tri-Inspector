using UnityEditor;
using UnityEngine;

namespace TriInspector.Editors
{
#if TRI_INSPECTOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    internal sealed class MonoBehaviourEditor : TriEditor
    {
    }
#endif
}