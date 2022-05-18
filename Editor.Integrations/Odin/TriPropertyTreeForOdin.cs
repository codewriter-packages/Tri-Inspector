using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using TriInspector.Elements;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector.Editor.Integrations.Odin
{
    public class TriPropertyTreeForOdin<T> : ITriPropertyParent, ITriPropertyTree
    {
        private readonly IPropertyValueEntry<T> _odinValueEntry;
        private readonly InspectorProperty _odinProperty;
        private readonly IReadOnlyList<TriProperty> _triProperties;
        private readonly TriInspectorElement _triInspectorElement;
        private readonly SerializedProperty _serializedProperty;

        private bool _validationRequired;

        public TriPropertyTreeForOdin(IPropertyValueEntry<T> odinValueEntry)
        {
            _odinValueEntry = odinValueEntry;
            _odinProperty = odinValueEntry.Property;

            TargetObjectType = _odinProperty.Tree.TargetType;
            TargetsCount = _odinProperty.Tree.WeakTargets.Count;
            TargetIsPersistent = _odinProperty.Tree.WeakTargets[0] is Object obj &&
                                 obj != null && EditorUtility.IsPersistent(obj);

            _serializedProperty = _odinProperty.Tree.GetUnityPropertyForPath(_odinProperty.Path, out _);

            UpdateAfterValueModification();

            _triProperties = TriTypeDefinition.GetCached(odinValueEntry.TypeOfValue)
                .Properties
                .Select((propertyDefinition, index) =>
                {
                    var serializedProperty = _serializedProperty.FindPropertyRelative(propertyDefinition.Name);
                    return new TriProperty(this, this, propertyDefinition, index, serializedProperty);
                })
                .ToList();

            _triInspectorElement = new TriInspectorElement(odinValueEntry.TypeOfValue, _triProperties);
            _triInspectorElement.AttachInternal();

            _triProperties.ForEach(it => it.Update());
            _triProperties.ForEach(it => it.RunValidation());
        }

        public void Draw()
        {
            UpdateEmittedScriptableObject();
            _triProperties.ForEach(it => it.Update());

            if (_validationRequired)
            {
                _validationRequired = false;

                _triProperties.ForEach(it => it.RunValidation());
            }

            _triInspectorElement.Update();
            var width = EditorGUIUtility.currentViewWidth;
            var height = _triInspectorElement.GetHeight(width);
            var rect = GUILayoutUtility.GetRect(width, height);
            _triInspectorElement.OnGUI(rect);
        }

        public Type TargetObjectType { get; }
        public int TargetsCount { get; }
        public bool TargetIsPersistent { get; }

        public void Dispose()
        {
            _triInspectorElement?.DetachInternal();
        }

        public void ForceCreateUndoGroup()
        {
            _odinProperty.RecordForUndo(forceCompleteObjectUndo: true);
            Undo.FlushUndoRecordObjects();
        }

        public void PrepareForValueModification()
        {
            var dirty = false;
            dirty |= _odinValueEntry.ApplyChanges();
            dirty |= _serializedProperty.serializedObject.ApplyModifiedProperties();
            dirty |= ApplyEmittedScriptableObject();

            if (dirty)
            {
                _validationRequired = true;
                GUIHelper.RequestRepaint();
            }
        }

        public void UpdateAfterValueModification()
        {
            UpdateEmittedScriptableObject();

            _serializedProperty.serializedObject.SetIsDifferentCacheDirty();
            _serializedProperty.serializedObject.Update();

            _odinProperty.Update();
        }

        public void RequestRepaint()
        {
            GUIHelper.RequestRepaint();
        }

        public object GetValue(int targetIndex)
        {
            return _odinValueEntry.Values[targetIndex];
        }

        public void NotifyValueChanged(TriProperty property)
        {
            ApplyEmittedScriptableObject();

            _odinValueEntry.Values.ForceMarkDirty();
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