using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Editors
{
    public abstract class TriEditor : Editor
    {
        private TriEditorCore _core;

        protected virtual void OnEnable()
        {
            _core = new TriEditorCore(this);
        }

        protected virtual void OnDisable()
        {
            _core.Dispose();
        }

        public override VisualElement CreateInspectorGUI()
        {
            return _core.CreateVisualElement();
        }

        public override void OnInspectorGUI()
        {
            DrawImguiWarning();
        }

        public static void DrawImguiWarning()
        {
            EditorGUILayout.HelpBox(
                "TriInspector 2.0 does not support IMGUI. " +
                "Migrate your custom editor to UI Toolkit or " +
                "downgrade TriInspector to version 1.x.x", MessageType.Warning);
        }
    }
}