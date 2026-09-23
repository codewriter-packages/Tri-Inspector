using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TriInspector.Editor.Integrations.Odin
{
    public sealed class TriPropertyTreeForOdin<T> : TriPropertyTree
    {
        private readonly IPropertyValueEntry<T> _odinValueEntry;
        private readonly InspectorProperty _odinProperty;
        private readonly SerializedProperty _serializedProperty;

        public TriPropertyTreeForOdin(IPropertyValueEntry<T> odinValueEntry)
        {
            _odinValueEntry = odinValueEntry;
            _odinProperty = odinValueEntry.Property;
            _serializedProperty = _odinProperty.Tree.GetUnityPropertyForPath(_odinProperty.Path, out _);

            TargetObjectType = _odinProperty.Tree.TargetType;
            TargetsCount = _odinProperty.Tree.WeakTargets.Count;
            TargetIsPersistent = _odinProperty.Tree.WeakTargets[0] is Object obj &&
                                 obj != null && EditorUtility.IsPersistent(obj);


            Update(forceUpdate: true);

            RootPropertyDefinition = new TriPropertyDefinition(
                memberInfo: odinValueEntry.Property.Info.GetMemberInfo(),
                ownerType: odinValueEntry.Property.Info.TypeOfOwner,
                order: -1,
                fieldName: odinValueEntry.Property.Name,
                fieldType: odinValueEntry.TypeOfValue,
                valueGetter: (self, targetIndex) => _odinValueEntry.Values[targetIndex],
                valueSetter: (self, targetIndex, value) =>
                {
                    _odinValueEntry.Values[targetIndex] = (T) value;
                    return null;
                },
                attributes: new List<Attribute>(),
                isArrayElement: false
            );
            RootProperty = new TriProperty(this, null, RootPropertyDefinition, -1, _serializedProperty);

            RootProperty.ValueChanged += OnPropertyChanged;
            RootProperty.ChildValueChanged += OnPropertyChanged;
        }

        public override void Dispose()
        {
            RootProperty.ChildValueChanged -= OnPropertyChanged;
            RootProperty.ValueChanged -= OnPropertyChanged;

            base.Dispose();
        }

        public override void Update(bool forceUpdate = false)
        {
            if (_serializedProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                var targetObjects = _serializedProperty.serializedObject.targetObjects;
                for (var index = 0; index < targetObjects.Length; ++index)
                {
                    ((EmittedScriptableObject<T>) targetObjects[index]).SetValue(_odinValueEntry.Values[index]);
                }

                _serializedProperty.serializedObject.Update();
            }

            _odinProperty.Update(forceUpdate);

            base.Update(forceUpdate);
        }

        public override bool ApplyChanges()
        {
            ApplyEmittedScriptableObject();

            var changed = base.ApplyChanges();
            changed |= _odinValueEntry.ApplyChanges();
            return changed;
        }

        public override void ForceCreateUndoGroup()
        {
            _odinProperty.RecordForUndo(forceCompleteObjectUndo: true);
        }

        private void OnPropertyChanged(TriProperty changedProperty)
        {
            ApplyEmittedScriptableObject();

            _odinValueEntry.Values.ForceMarkDirty();

            RequestValidation();
            RequestRepaint();
        }

        private void ApplyEmittedScriptableObject()
        {
            if (_serializedProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                _serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                var targetObjects = _serializedProperty.serializedObject.targetObjects;
                for (var index = 0; index < targetObjects.Length; ++index)
                {
                    _odinValueEntry.Values[index] = ((EmittedScriptableObject<T>) targetObjects[index]).GetValue();
                }
            }
        }
    }
}