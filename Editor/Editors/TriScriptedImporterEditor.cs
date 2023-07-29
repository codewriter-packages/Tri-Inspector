using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine.UIElements;

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

        public override VisualElement CreateInspectorGUI()
        {
            return TriEditor.CreateInspector(root => OnInspectorGUI(root));
        }

        public override void OnInspectorGUI()
        {
            OnInspectorGUI(null);
        }

        private void OnInspectorGUI(VisualElement root)
        {
            TriEditor.OnInspectorGUI(this, ref _inspector, root);

            if (extraDataType != null)
            {
                EditorProxy.DoDrawDefaultInspector(extraDataSerializedObject);
            }

            ApplyRevertGUI();
        }
    }
}