using System.Linq;
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

            Properties = TriTypeDefinition.GetCached(odinValueEntry.TypeOfValue)
                .Properties
                .Select((propertyDefinition, index) =>
                {
                    var serializedProperty = _serializedProperty.FindPropertyRelative(propertyDefinition.Name);
                    return new TriProperty(this, this, propertyDefinition, index, serializedProperty);
                })
                .ToList();
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

        public override object GetValue(int targetIndex)
        {
            return _odinValueEntry.Values[targetIndex];
        }

        public override void NotifyValueChanged(TriProperty property)
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