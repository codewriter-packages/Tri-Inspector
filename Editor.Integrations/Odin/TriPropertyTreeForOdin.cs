using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TriInspector.Editor.Integrations.Odin
{
    public class TriPropertyTreeForOdin<T> : TriPropertyTree
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

            UpdateEmittedScriptableObject();

            _serializedProperty.serializedObject.SetIsDifferentCacheDirty();
            _serializedProperty.serializedObject.Update();

            _odinProperty.Update();

            RootPropertyDefinition = new TriPropertyDefinition(
                memberInfo: odinValueEntry.Property.Info.GetMemberInfo(),
                order: -1,
                fieldName: odinValueEntry.Property.Name,
                fieldType: odinValueEntry.TypeOfValue,
                valueGetter: (self, targetIndex) => _odinValueEntry.Values[targetIndex],
                valueSetter: (self, targetIndex, value) =>
                {
                    _odinValueEntry.Values[targetIndex] = (T) value;
                    return null;
                },
                isArrayElement: false
            );
            RootProperty = new TriProperty(this, null, RootPropertyDefinition, -1, _serializedProperty);
            RootProperty.ValueChanged += OnRootPropertyChanged;
        }

        public override void Dispose()
        {
            RootProperty.ValueChanged -= OnRootPropertyChanged;

            base.Dispose();
        }

        public override void Update()
        {
            UpdateEmittedScriptableObject();

            base.Update();
        }

        public override void ForceCreateUndoGroup()
        {
            _odinProperty.RecordForUndo(forceCompleteObjectUndo: true);
            Undo.FlushUndoRecordObjects();
        }

        public override void PrepareForValueModification()
        {
            var dirty = false;
            dirty |= _odinValueEntry.ApplyChanges();
            dirty |= _serializedProperty.serializedObject.ApplyModifiedProperties();
            dirty |= ApplyEmittedScriptableObject();

            if (dirty)
            {
                RequestValidation();
                RequestRepaint();
            }
        }

        public override void UpdateAfterValueModification()
        {
            UpdateEmittedScriptableObject();

            _serializedProperty.serializedObject.SetIsDifferentCacheDirty();
            _serializedProperty.serializedObject.Update();

            _odinProperty.Update();
        }

        public override void RequestRepaint()
        {
            base.RequestRepaint();

            GUIHelper.RequestRepaint();
        }

        private void OnRootPropertyChanged(TriProperty _, TriProperty changedProperty)
        {
            ApplyEmittedScriptableObject();

            _odinValueEntry.Values.ForceMarkDirty();

            RequestValidation();
        }

        private void UpdateEmittedScriptableObject()
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
        }

        private bool ApplyEmittedScriptableObject()
        {
            var dirty = false;

            if (_serializedProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                dirty = _serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                var targetObjects = _serializedProperty.serializedObject.targetObjects;
                for (var index = 0; index < targetObjects.Length; ++index)
                {
                    _odinValueEntry.Values[index] = ((EmittedScriptableObject<T>) targetObjects[index]).GetValue();
                }
            }

            return dirty;
        }
    }
}