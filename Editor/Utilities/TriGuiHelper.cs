﻿using System;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    public static class TriGuiHelper
    {
        public static LabelWidthScope PushLabelWidth(float labelWidth)
        {
            return new LabelWidthScope(labelWidth);
        }

        public static IndentLevelScope PushIndentLevel(int indent = 1)
        {
            return new IndentLevelScope(indent);
        }

        public static GuiColorScope PushColor(Color color)
        {
            return new GuiColorScope(color);
        }

        public readonly struct LabelWidthScope : IDisposable
        {
            private readonly float _oldLabelWidth;

            public LabelWidthScope(float labelWidth)
            {
                _oldLabelWidth = EditorGUIUtility.labelWidth;

                if (labelWidth > 0)
                {
                    EditorGUIUtility.labelWidth = labelWidth;
                }
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = _oldLabelWidth;
            }
        }

        public readonly struct IndentLevelScope : IDisposable
        {
            private readonly int _oldIndentLevel;

            public IndentLevelScope(int indentLevel)
            {
                _oldIndentLevel = EditorGUI.indentLevel;

                EditorGUI.indentLevel += indentLevel;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = _oldIndentLevel;
            }
        }

        public readonly struct GuiColorScope : IDisposable
        {
            private readonly Color _oldColor;

            public GuiColorScope(Color color)
            {
                _oldColor = GUI.color;

                GUI.color = color;
            }

            public void Dispose()
            {
                GUI.color = _oldColor;
            }
        }
    }
}