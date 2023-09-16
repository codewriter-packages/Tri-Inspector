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
        private TriEditorCore _core;

        public override void OnEnable()
        {
            base.OnEnable();

            _core = new TriEditorCore(this);
        }

        public override void OnDisable()
        {
            _core.Dispose();

            base.OnDisable();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(_core.CreateVisualElement());
            root.Add(new IMGUIContainer(() => DoImporterDefaultGUI()));

            return root;
        }

        private void DoImporterDefaultGUI()
        {
            if (extraDataType != null)
            {
                EditorProxy.DoDrawDefaultInspector(extraDataSerializedObject);
            }

            ApplyRevertGUI();
        }
    }
}