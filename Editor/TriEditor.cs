using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), editorForChildClasses: true, isFallback = true)]
    public class TriEditor : Editor
    {
        private TriPropertyTreeForSerializedObject _inspector;

        private void OnDisable()
        {
            _inspector?.Dispose();
            _inspector = null;
        }

        public override void OnInspectorGUI()
        {
            if (_inspector == null)
            {
                if (serializedObject.targetObjects.Length == 0)
                {
                    return;
                }

                if (serializedObject.targetObject == null)
                {
                    EditorGUILayout.HelpBox("Script is missing", MessageType.Warning);
                    return;
                }

                _inspector = new TriPropertyTreeForSerializedObject(serializedObject);
            }

            serializedObject.UpdateIfRequiredOrScript();

            _inspector.Update();

            if (_inspector.ValidationRequired)
            {
                _inspector.RunValidation();
            }

            using (TriGuiHelper.PushEditorTarget(target))
            {
                _inspector.Draw();
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                _inspector.RequestValidation();
            }

            if (_inspector.RepaintRequired)
            {
                Repaint();
            }
        }
    }
}