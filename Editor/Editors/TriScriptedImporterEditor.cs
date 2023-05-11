using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace TriInspector.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptedImporter), editorForChildClasses: true)]
    public sealed class TriScriptedImporterEditor : ScriptedImporterEditor
    {
        private TriPropertyTreeForSerializedObject _inspector;

        public override void OnDisable()
        {
            TriEditor.OnDisable(this, ref _inspector);

            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            TriEditor.OnInspectorGUI(this, ref _inspector);

            if (extraDataType != null)
            {
                EditorProxy.DoDrawDefaultInspector(extraDataSerializedObject);
            }

            ApplyRevertGUI();
        }
    }
}