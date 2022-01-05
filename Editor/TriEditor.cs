using TriInspector.Utilities;
using UnityEditor;
using UnityEngine.Profiling;

namespace TriInspector
{
    public abstract class TriEditor : Editor
    {
        private TriPropertyTree _inspector;

        private void OnEnable()
        {
            if (serializedObject.targetObject != null)
            {
                _inspector = TriPropertyTree.Create(serializedObject);
            }
        }

        private void OnDisable()
        {
            _inspector?.Destroy();
        }

        public override void OnInspectorGUI()
        {
            if (_inspector == null)
            {
                DrawDefaultInspector();
                return;
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

            TriGuiHelper.PushEditor(this);
            Profiler.BeginSample("TriInspector.DoLayout()");
            try
            {
                _inspector.DoLayout();
            }
            finally
            {
                Profiler.EndSample();
                TriGuiHelper.PopEditor(this);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}