using UnityEditor;
using UnityEngine;

namespace TriInspector.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    internal sealed class MonoBehaviourEditor : TriEditor
    {
    }
}