using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;

namespace TriInspector
{
    public sealed class TriPropertyTreeForSerializedObject : TriPropertyTree
    {
        private readonly SerializedObject _serializedObject;

        public TriPropertyTreeForSerializedObject([NotNull] SerializedObject serializedObject)
        {
            _serializedObject = serializedObject ?? throw new ArgumentNullException(nameof(serializedObject));

            TargetObjectType = _serializedObject.targetObject.GetType();
            TargetsCount = _serializedObject.targetObjects.Length;
            TargetIsPersistent = _serializedObject.targetObject is var targetObject &&
                                 targetObject != null && !EditorUtility.IsPersistent(targetObject);

            Properties = TriTypeDefinition.GetCached(TargetObjectType)
                .Properties
                .Select((propertyDefinition, index) =>
                {
                    var serializedProperty = serializedObject.FindProperty(propertyDefinition.Name);
                    return new TriProperty(this, this, propertyDefinition, index, serializedProperty);
                })
                .ToList();
        }

        public override object GetValue(int targetIndex)
        {
            return _serializedObject.targetObjects[targetIndex];
        }

        public override void ForceCreateUndoGroup()
        {
            Undo.RegisterCompleteObjectUndo(_serializedObject.targetObjects, "Inspector");
            Undo.FlushUndoRecordObjects();
        }

        public override void PrepareForValueModification()
        {
            if (_serializedObject.ApplyModifiedProperties())
            {
                RequestValidation();
                RequestRepaint();
            }
        }

        public override void UpdateAfterValueModification()
        {
            _serializedObject.SetIsDifferentCacheDirty();
            _serializedObject.Update();
        }

        public override void NotifyValueChanged(TriProperty property)
        {
            foreach (var targetObject in _serializedObject.targetObjects)
            {
                EditorUtility.SetDirty(targetObject);
            }

            RequestValidation();
        }
    }
}