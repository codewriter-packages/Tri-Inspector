using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TriInspector.Elements;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector
{
    public sealed class TriPropertyTree : ITriPropertyParent
    {
        private readonly TriEditorMode _mode;
        private readonly TriInspectorElement _inspectorElement;

        private TriPropertyTree([NotNull] SerializedObject serializedObject, TriEditorMode mode)
        {
            SerializedObject = serializedObject ?? throw new ArgumentNullException(nameof(serializedObject));
            TargetObjects = serializedObject.targetObjects;
            TargetObjectType = TargetObjects[0].GetType();
            Root = this;

            Properties = TriTypeDefinition.GetCached(TargetObjectType)
                .Properties
                .Select((propertyDefinition, index) =>
                {
                    var serializedProperty = serializedObject.FindProperty(propertyDefinition.Name);
                    return new TriProperty(this, this, propertyDefinition, index, serializedProperty);
                })
                .ToList();

            _mode = mode;
            _inspectorElement = new TriInspectorElement(this);
            _inspectorElement.AttachInternal();
        }

        [PublicAPI]
        public IReadOnlyList<TriProperty> Properties { get; }

        [PublicAPI]
        public Object[] TargetObjects { get; }

        [PublicAPI]
        public Type TargetObjectType { get; }

        [PublicAPI]
        public SerializedObject SerializedObject { get; }

        public TriPropertyTree Root { get; }

        public bool IsInlineEditor => (_mode & TriEditorMode.InlineEditor) != 0;

        internal bool RepaintRequired { get; set; }

        object ITriPropertyParent.GetValue(int targetIndex) => TargetObjects[targetIndex];

        internal static TriPropertyTree Create(SerializedObject scriptableObject,
            TriEditorMode mode = TriEditorMode.None)
        {
            return new TriPropertyTree(scriptableObject, mode);
        }

        internal void Destroy()
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

            _inspectorElement.Update();
        }

        internal void DoLayout()
        {
            var width = EditorGUIUtility.currentViewWidth;
            var height = _inspectorElement.GetHeight(width);
            var rect = GUILayoutUtility.GetRect(width, height);
            _inspectorElement.OnGUI(rect);
        }

        public void RequestRepaint()
        {
            RepaintRequired = true;
        }
    }

    [Flags]
    public enum TriEditorMode
    {
        None = 0,
        InlineEditor = 1 << 0,
    }
}