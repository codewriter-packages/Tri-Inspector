using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Editors
{
    public abstract class TriEditor : Editor
    {
        private TriPropertyTreeForSerializedObject _inspector;

        private void OnDisable()
        {
            OnDisable(this, ref _inspector);
        }

        public override void OnInspectorGUI()
        {
            OnInspectorGUI(this, ref _inspector);
        }

        public static void OnDisable(Editor editor, ref TriPropertyTreeForSerializedObject inspector)
        {
            inspector?.Dispose();
            inspector = null;
        }

        public static void OnInspectorGUI(Editor editor,
            ref TriPropertyTreeForSerializedObject inspector)
        {
            var serializedObject = editor.serializedObject;

            if (serializedObject.targetObjects.Length == 0)
            {
                return;
            }

            if (serializedObject.targetObject == null)
            {
                EditorGUILayout.HelpBox("Script is missing", MessageType.Warning);
                return;
            }

            foreach (var targetObject in serializedObject.targetObjects)
            {
                if (TriGuiHelper.IsEditorTargetPushed(targetObject))
                {
                    GUILayout.Label("Recursive inline editors not supported");
                    return;
                }
            }

            if (inspector == null)
            {
                inspector = new TriPropertyTreeForSerializedObject(serializedObject);
            }

            serializedObject.UpdateIfRequiredOrScript();

            inspector.Update();
            inspector.RunValidationIfRequired();

            using (TriGuiHelper.PushEditorTarget(serializedObject.targetObject))
            {
                inspector.Draw();
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                inspector.RequestValidation();
            }

            if (inspector.RepaintRequired)
            {
                editor.Repaint();
            }
        }
    }
}