using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    public static class TriGuiHelper
    {
        private static readonly Stack<EditorData> EditorStack = new Stack<EditorData>();
        private static readonly Stack<float> LabelWidthStack = new Stack<float>();
        private static readonly Stack<int> IndentLevelStack = new Stack<int>();
        private static readonly Stack<Color> ColorStack = new Stack<Color>();

        private static void CleanUp()
        {
            EditorStack.Clear();
            LabelWidthStack.Clear();
            IndentLevelStack.Clear();
        }

        public static int PushedEditorCount => EditorStack.Count;

        public static bool IsEditorForObjectPushed(Object target)
        {
            foreach (var it in EditorStack)
            {
                if (it.editor.target == target)
                {
                    return true;
                }
            }

            return false;
        }

        public static void PushEditor(Editor editor)
        {
            EditorStack.Push(new EditorData
            {
                editor = editor,
                labelWidthStackSize = LabelWidthStack.Count,
                indentLevelStackSize = IndentLevelStack.Count,
                ColorStackSize = ColorStack.Count,
            });
        }

        public static void PopEditor(Editor editor)
        {
            if (EditorStack.Count == 0)
            {
                Debug.LogError("No editor in stack");
                return;
            }

            var data = EditorStack.Pop();

            if (data.editor != editor)
            {
                Debug.LogError($"Editor pop mismatch: {editor}");
            }

            CheckSizeMismatch(data.labelWidthStackSize, LabelWidthStack, nameof(LabelWidthStack), editor);
            CheckSizeMismatch(data.indentLevelStackSize, IndentLevelStack, nameof(IndentLevelStack), editor);
            CheckSizeMismatch(data.ColorStackSize, ColorStack, nameof(ColorStack), editor);

            if (EditorStack.Count == 0)
            {
                CleanUp();
            }
        }

        public static void PushLabelWidth(float labelWidth)
        {
            LabelWidthStack.Push(EditorGUIUtility.labelWidth);

            if (labelWidth > 0)
            {
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }

        public static void PopLabelWidth()
        {
            if (LabelWidthStack.Count == 0)
            {
                Debug.LogError("No label width in stack");
                return;
            }

            EditorGUIUtility.labelWidth = LabelWidthStack.Pop();
        }

        public static void PushIndentLevel(int indent = 1)
        {
            IndentLevelStack.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel += indent;
        }

        public static void PopIndentLevel()
        {
            if (IndentLevelStack.Count == 0)
            {
                Debug.LogError("No indent level in stack");
                return;
            }

            EditorGUI.indentLevel = IndentLevelStack.Pop();
        }

        public static void PushColor(Color color)
        {
            ColorStack.Push(GUI.color);
            GUI.color = color;
        }

        public static void PopColor()
        {
            if (ColorStack.Count == 0)
            {
                Debug.LogError("No color in stack");
                return;
            }

            GUI.color = ColorStack.Pop();
        }

        private static void CheckSizeMismatch<T>(int expectedSize, Stack<T> stack, string stackName, Editor editor)
        {
            if (expectedSize != stack.Count)
            {
                Debug.LogError($"{stackName} size mismatch in {editor.name} editor");
            }
        }

        private struct EditorData
        {
            public Editor editor;
            public int labelWidthStackSize;
            public int indentLevelStackSize;
            public int ColorStackSize;
        }
    }
}