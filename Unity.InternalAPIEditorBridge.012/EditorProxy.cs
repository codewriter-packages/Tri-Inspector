using UnityEditor;

namespace TriInspectorUnityInternalBridge
{
    internal static class EditorProxy
    {
        public static void DoDrawDefaultInspector(SerializedObject obj)
        {
            Editor.DoDrawDefaultInspector(obj);
        }
    }
}