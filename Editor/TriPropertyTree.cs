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
        private readonly TriInspectorElement _inspectorElement;

        private TriPropertyTree(SerializedObject serializedObject)
        {
            TargetObjects = serializedObject.targetObjects;
            TargetObjectType = TargetObjects[0].GetType();
            Root = this;

            Properties = TriTypeDefinition.GetCached(TargetObjectType)
                .Properties
                .Select(propertyDefinition =>
                {
                    var serializedProperty = serializedObject.FindProperty(propertyDefinition.Name);
                    return new TriProperty(this, this, propertyDefinition, serializedProperty);
                })
                .ToList();

            _inspectorElement = new TriInspectorElement(this);
            _inspectorElement.AttachInternal();
        }

        [PublicAPI]
        public IReadOnlyList<TriProperty> Properties { get; }

        [PublicAPI]
        public Object[] TargetObjects { get; }

        [PublicAPI]
        public Type TargetObjectType { get; }

        public TriPropertyTree Root { get; }

        object ITriPropertyParent.GetValue(int targetIndex) => TargetObjects[targetIndex];

        void ITriPropertyParent.ApplyChildValueModifications(int targetIndex)
        {
            EditorUtility.SetDirty(TargetObjects[targetIndex]);
        }

        internal static TriPropertyTree Create(SerializedObject scriptableObject)
        {
            return new TriPropertyTree(scriptableObject);
        }

        internal void Destroy()
        {
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
    }
}