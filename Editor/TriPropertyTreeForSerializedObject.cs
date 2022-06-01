using System;
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

            RootPropertyDefinition = new TriPropertyDefinition(
                memberInfo: null,
                order: -1,
                fieldName: "ROOT",
                fieldType: TargetObjectType,
                valueGetter: (self, targetIndex) => _serializedObject.targetObjects[targetIndex],
                valueSetter: (self, targetIndex, value) => _serializedObject.targetObjects[targetIndex],
                isArrayElement: false);

            RootProperty = new TriProperty(this, null, RootPropertyDefinition, serializedObject);
            RootProperty.ValueChanged += OnRootPropertyChanged;
        }

        public override void Dispose()
        {
            RootProperty.ValueChanged -= OnRootPropertyChanged;

            base.Dispose();
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

        private void OnRootPropertyChanged(TriProperty root, TriProperty changedProperty)
        {
            foreach (var targetObject in _serializedObject.targetObjects)
            {
                EditorUtility.SetDirty(targetObject);
            }

            RequestValidation();
        }
    }
}