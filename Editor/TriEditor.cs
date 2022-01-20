using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace TriInspector
{
    public abstract class TriEditor : Editor
    {
        private static readonly Stack<Editor> EditorStack = new Stack<Editor>();

        private TriPropertyTree _inspector;

        private void OnEnable()
        {
            var mode = TriEditorMode.None;

            var isInlineEditor = EditorStack.Count > 0;
            if (isInlineEditor)
            {
                mode |= TriEditorMode.InlineEditor;
            }

            if (serializedObject.targetObject != null)
            {
                _inspector = TriPropertyTree.Create(serializedObject, mode);
            }
        }

        private void OnDisable()
        {
            _inspector?.Destroy();
            _inspector = null;
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

            Profiler.BeginSample("TriInspector.RunValidation()");
            try
            {
                if (_inspector.ValidationRequired)
                {
                    _inspector.ValidationRequired = false;

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
                _inspector.DoLayout();
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
                _inspector.RepaintRequired = false;

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