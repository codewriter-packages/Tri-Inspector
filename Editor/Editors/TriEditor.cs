using System;
using System.Collections.Generic;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Editors
{
    public abstract class TriEditor : Editor
    {
        internal static readonly Dictionary<TriPropertyTree, VisualElement> UiElementsRoots
            = new Dictionary<TriPropertyTree, VisualElement>();

        private TriPropertyTreeForSerializedObject _inspector;

        private void OnDisable()
        {
            OnDisable(this, ref _inspector);
        }

        public override VisualElement CreateInspectorGUI()
        {
            return CreateInspector(root => OnInspectorGUI(this, ref _inspector, root));
        }

        public override void OnInspectorGUI()
        {
            OnInspectorGUI(this, ref _inspector);
        }

        public static void OnDisable(Editor editor, ref TriPropertyTreeForSerializedObject inspector)
        {
            if (inspector != null)
            {
                UiElementsRoots.Remove(inspector);

                inspector.Dispose();
            }

            inspector = null;
        }

        public static VisualElement CreateInspector(Action<VisualElement> onGui)
        {
            var container = new VisualElement();
            var root = new VisualElement()
            {
                style =
                {
                    position = Position.Absolute,
                },
            };

            container.Add(new IMGUIContainer(() =>
            {
                const float labelExtraPadding = 2;
                const float labelWidthRatio = 0.45f;
                const float labelMinWidth = 120;

                var space = container.resolvedStyle.left + container.resolvedStyle.right + labelExtraPadding;

                EditorGUIUtility.hierarchyMode = false;
                EditorGUIUtility.labelWidth = Mathf.Max(labelMinWidth,
                    container.resolvedStyle.width * labelWidthRatio - space);

                onGui?.Invoke(root);
            }));

            container.Add(root);

            return container;
        }

        public static void OnInspectorGUI(Editor editor,
            ref TriPropertyTreeForSerializedObject inspector, VisualElement visualRoot = null)
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

            if (visualRoot != null)
            {
                UiElementsRoots[inspector] = visualRoot;
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