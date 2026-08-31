using UnityEditor;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace TriInspector.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptedImporter), editorForChildClasses: true)]
    internal sealed class TriScriptedImporterFallbackEditor : TriScriptedImporterEditor
    {
    }
}