﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TriInspector.Elements;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector
{
    public sealed class TriPropertyTree : ITriPropertyParent, ITriPropertyTree
    {
        private readonly TriEditorMode _mode;
        private readonly TriInspectorElement _inspectorElement;

        private TriPropertyTree([NotNull] SerializedObject serializedObject, TriEditorMode mode)
        {
            SerializedObject = serializedObject ?? throw new ArgumentNullException(nameof(serializedObject));
            TargetObjects = serializedObject.targetObjects;
            TargetObjectType = TargetObjects[0].GetType();

            Properties = TriTypeDefinition.GetCached(TargetObjectType)
                .Properties
                .Select((propertyDefinition, index) =>
                {
                    var serializedProperty = serializedObject.FindProperty(propertyDefinition.Name);
                    return new TriProperty(this, this, propertyDefinition, index, serializedProperty);
                })
                .ToList();

            _mode = mode;
            _inspectorElement = new TriInspectorElement(TargetObjectType, Properties);
            _inspectorElement.AttachInternal();

            Update();
            RunValidation();
        }

        [PublicAPI]
        public IReadOnlyList<TriProperty> Properties { get; }

        [PublicAPI]
        public Object[] TargetObjects { get; }

        [PublicAPI]
        public Type TargetObjectType { get; }

        [PublicAPI]
        public int TargetsCount => TargetObjects.Length;

        private SerializedObject SerializedObject { get; }

        public bool IsInlineEditor => (_mode & TriEditorMode.InlineEditor) != 0;

        public bool TargetIsPersistent => TargetObjects[0] is var targetObject &&
                                          targetObject != null && !EditorUtility.IsPersistent(targetObject);

        internal bool RepaintRequired { get; set; }
        internal bool ValidationRequired { get; set; }

        object ITriPropertyParent.GetValue(int targetIndex) => TargetObjects[targetIndex];

        internal static TriPropertyTree Create(SerializedObject scriptableObject,
            TriEditorMode mode = TriEditorMode.None)
        {
            return new TriPropertyTree(scriptableObject, mode);
        }

        public void Dispose()
        {
            if (!_inspectorElement.IsAttached)
            {
                return;
            }

            _inspectorElement.DetachInternal();
        }

        internal void Update()
        {
            foreach (var property in Properties)
            {
                property.Update();
            }
        }

        internal void RunValidation()
        {
            foreach (var property in Properties)
            {
                property.RunValidation();
            }

            RequestRepaint();
        }

        internal void DoLayout()
        {
            _inspectorElement.Update();
            var width = EditorGUIUtility.currentViewWidth;
            var height = _inspectorElement.GetHeight(width);
            var rect = GUILayoutUtility.GetRect(width, height);
            _inspectorElement.OnGUI(rect);
        }

        public void UpdateAfterValueModification()
        {
            SerializedObject.SetIsDifferentCacheDirty();
            SerializedObject.Update();
        }

        public void PrepareForValueModification()
        {
            if (SerializedObject.ApplyModifiedProperties())
            {
                RequestValidation();
                RequestRepaint();
            }

            Undo.RegisterCompleteObjectUndo(TargetObjects, "Inspector");
            Undo.FlushUndoRecordObjects();
        }

        public void NotifyValueChanged(TriProperty property)
        {
            foreach (var targetObject in TargetObjects)
            {
                EditorUtility.SetDirty(targetObject);
            }

            RequestValidation();
        }

        public void RequestRepaint()
        {
            RepaintRequired = true;
        }

        public void RequestValidation()
        {
            ValidationRequired = true;
        }
    }

    public interface ITriPropertyTree : IDisposable
    {
        Type TargetObjectType { get; }
        int TargetsCount { get; }
        bool TargetIsPersistent { get; }

        void PrepareForValueModification();
        void UpdateAfterValueModification();
        void RequestRepaint();
    }

    [Flags]
    public enum TriEditorMode
    {
        None = 0,
        InlineEditor = 1 << 0,
    }
}