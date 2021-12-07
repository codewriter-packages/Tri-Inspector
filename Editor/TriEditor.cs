using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace TriInspector
{
    public abstract class TriEditor : Editor
    {
        private bool _drawDefaultInspector;
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

            DrawInspectorModeHeader();

            if (_drawDefaultInspector)
            {
                DrawDefaultInspector();
            }
            else
            {
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
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInspectorModeHeader()
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!_drawDefaultInspector, "Tri Inspector", EditorStyles.miniButtonLeft))
            {
                _drawDefaultInspector = false;
            }

            if (GUILayout.Toggle(_drawDefaultInspector, "Default Inspector", EditorStyles.miniButtonRight))
            {
                _drawDefaultInspector = true;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
            GUILayout.Space(10);
        }
    }
}