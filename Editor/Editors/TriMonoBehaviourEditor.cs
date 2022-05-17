using UnityEditor;

namespace TriInspector.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TriMonoBehaviour), true, isFallback = true)]
    internal sealed class TriMonoBehaviourEditor : TriEditor
    {
        
    }
}