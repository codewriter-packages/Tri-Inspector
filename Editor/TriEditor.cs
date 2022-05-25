using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

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
                _inspector.Initialize();
            }

            serializedObject.UpdateIfRequiredOrScript();

            Profiler.BeginSample("TriInspector.Update()");
            try
            {
                _inspector.Update();
            }
            finally
            {
                Profiler.EndSample();
            }

            Profiler.BeginSample("TriInspector.RunValidation()");
            try
            {
                if (_inspector.ValidationRequired)
                {
                    _inspector.RunValidation();
                }
            }
            finally
            {
                Profiler.EndSample();
            }

            Profiler.BeginSample("TriInspector.DoLayout()");
            try
            {
                using (TriGuiHelper.PushEditorTarget(target))
                {
                    _inspector.Draw();
                }
            }
            finally
            {
                Profiler.EndSample();
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