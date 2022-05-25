using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace TriInspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), editorForChildClasses: true, isFallback = true)]
    public class TriEditor : Editor
    {
        private static readonly Stack<Editor> EditorStack = new Stack<Editor>();

        private TriPropertyTreeForSerializedObject _inspector;

        private TriEditorMode _editorMode;

        private void OnEnable()
        {
            _editorMode = TriEditorMode.None;

            var isInlineEditor = EditorStack.Count > 0;
            if (isInlineEditor)
            {
                _editorMode |= TriEditorMode.InlineEditor;
            }
        }

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
                _inspector.Initialize(_editorMode);
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

            EditorStack.Push(this);
            Profiler.BeginSample("TriInspector.DoLayout()");
            try
            {
                _inspector.Draw();
            }
            finally
            {
                Profiler.EndSample();
                EditorStack.Pop();
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

        internal static bool IsEditorForObjectPushed(Object targetObject)
        {
            foreach (var editor in EditorStack)
            {
                foreach (var editorTarget in editor.targets)
                {
                    if (editorTarget == targetObject)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}