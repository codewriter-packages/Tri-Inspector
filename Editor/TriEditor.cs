using TriInspector.Utilities;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace TriInspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true, isFallback = true)]
    internal sealed class TriMonoBehaviourEditor : TriEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), editorForChildClasses: true, isFallback = true)]
    internal sealed class TriScriptableObjectEditor : TriEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(AssetImporter), editorForChildClasses: true)]
    public sealed class TriAssetImporterEditor : AssetImporterEditor
    {
        private TriPropertyTreeForSerializedObject _inspector;

        public override void OnDisable()
        {
            base.OnDisable();
            _inspector?.Dispose();
            _inspector = null;
        }

        public override void OnInspectorGUI()
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

            if (TriGuiHelper.IsEditorTargetPushed(serializedObject.targetObject))
            {
                GUILayout.Label("Recursive inline editors not supported");
                return;
            }

            if (_inspector == null)
            {
                _inspector = new TriPropertyTreeForSerializedObject(serializedObject);
            }

            serializedObject.UpdateIfRequiredOrScript();

            _inspector.Update();
            _inspector.RunValidationIfRequired();

            using (TriGuiHelper.PushEditorTarget(target))
            {
                _inspector.Draw();
                ApplyRevertGUI();
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

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptedImporter), editorForChildClasses: true)]
    public sealed class TriScriptedImporterEditor : ScriptedImporterEditor
    {
        private TriPropertyTreeForSerializedObject _inspector;

        public override void OnDisable()
        {
            base.OnDisable();
            _inspector?.Dispose();
            _inspector = null;
        }

        public override void OnInspectorGUI()
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

            if (TriGuiHelper.IsEditorTargetPushed(serializedObject.targetObject))
            {
                GUILayout.Label("Recursive inline editors not supported");
                return;
            }

            if (_inspector == null)
            {
                _inspector = new TriPropertyTreeForSerializedObject(serializedObject);
            }

            serializedObject.UpdateIfRequiredOrScript();

            _inspector.Update();
            _inspector.RunValidationIfRequired();

            using (TriGuiHelper.PushEditorTarget(target))
            {
                _inspector.Draw();
                ApplyRevertGUI();
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
            if (serializedObject.targetObjects.Length == 0)
            {
                return;
            }

            if (serializedObject.targetObject == null)
            {
                EditorGUILayout.HelpBox("Script is missing", MessageType.Warning);
                return;
            }

            if (TriGuiHelper.IsEditorTargetPushed(serializedObject.targetObject))
            {
                GUILayout.Label("Recursive inline editors not supported");
                return;
            }

            if (_inspector == null)
            {
                _inspector = new TriPropertyTreeForSerializedObject(serializedObject);
            }

            serializedObject.UpdateIfRequiredOrScript();

            _inspector.Update();
            _inspector.RunValidationIfRequired();

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