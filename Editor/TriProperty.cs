using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    public sealed class TriProperty : ITriPropertyParent
    {
        private static readonly IReadOnlyList<TriValidationResult> EmptyValidationResults =
            new List<TriValidationResult>();

        private readonly TriPropertyDefinition _definition;
        private readonly int _propertyIndex;
        private readonly ITriPropertyParent _parent;
        [CanBeNull] private readonly SerializedProperty _serializedProperty;
        private List<TriProperty> _childrenProperties;
        private List<TriValidationResult> _validationResults;

        private GUIContent _displayNameBackingField;

        private string _isExpandedPrefsKey;

        internal TriProperty(
            TriPropertyTree propertyTree,
            ITriPropertyParent parent,
            TriPropertyDefinition definition,
            int propertyIndex,
            [CanBeNull] SerializedProperty serializedProperty)
        {
            _parent = parent;
            _definition = definition;
            _propertyIndex = propertyIndex;
            _serializedProperty = serializedProperty?.Copy();

            PropertyTree = propertyTree;
            PropertyType = GetPropertyType(this);

            Update();
        }

        [PublicAPI]
        public string RawName => _definition.Name;

        [PublicAPI]
        public string DisplayName => DisplayNameContent.text;

        [PublicAPI]
        public GUIContent DisplayNameContent
        {
            get
            {
                if (TriPropertyOverrideContext.Current != null)
                {
                    return TriPropertyOverrideContext.Current.GetDisplayName(this);
                }

                if (_displayNameBackingField == null)
                {
                    if (TryGetAttribute(out HideLabelAttribute _))
                    {
                        _displayNameBackingField = new GUIContent("");
                    }
                    else if (IsArrayElement)
                    {
                        _displayNameBackingField = new GUIContent($"{_definition.Name} {IndexInArray}");
                    }
                    else
                    {
                        _displayNameBackingField = new GUIContent(ObjectNames.NicifyVariableName(_definition.Name));
                    }
                }

                if (IsArrayElement)
                {
                }
                else
                {
                    if (TryGetAttribute(out LabelTextAttribute labelTextAttribute))
                    {
                        _displayNameBackingField.text = labelTextAttribute.Text;
                    }

                    if (TryGetAttribute(out PropertyTooltipAttribute tooltipAttribute))
                    {
                        _displayNameBackingField.tooltip = tooltipAttribute.Tooltip;
                    }
                }

                return _displayNameBackingField;
            }
        }

        [PublicAPI]
        public bool IsVisible
        {
            get
            {
                foreach (var processor in _definition.HideProcessors)
                {
                    if (processor.IsHidden(this))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [PublicAPI]
        public bool IsEnabled
        {
            get
            {
                if (_definition.IsReadOnly)
                {
                    return false;
                }

                foreach (var processor in _definition.DisableProcessors)
                {
                    if (processor.IsDisabled(this))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [PublicAPI]
        public MemberInfo MemberInfo => _definition.MemberInfo;

        [PublicAPI]
        public Type FieldType => _definition.FieldType;

        [PublicAPI]
        public Type ArrayElementType => _definition.ArrayElementType;

        [PublicAPI]
        public bool IsArrayElement => _definition.IsArrayElement;

        [PublicAPI]
        public bool IsArray => _definition.IsArray;

        public int IndexInArray => IsArrayElement
            ? _propertyIndex
            : throw new InvalidOperationException("Cannot read IndexInArray for !IsArrayElement");

        public IReadOnlyList<TriCustomDrawer> AllDrawers => _definition.Drawers;

        public ITriPropertyParent Parent => _parent;

        public bool HasValidators => _definition.Validators.Count != 0;

        public IReadOnlyList<TriValidationResult> ValidationResults =>
            _validationResults ?? EmptyValidationResults;

        [PublicAPI]
        public bool IsExpanded
        {
            get
            {
                if (_serializedProperty != null)
                {
                    return _serializedProperty.isExpanded;
                }

                if (_isExpandedPrefsKey == null)
                {
                    _isExpandedPrefsKey = $"TriInspector.expanded.{PropertyTree.TargetObjectType}.{MemberInfo}";
                }

                return EditorPrefs.GetBool(_isExpandedPrefsKey, false);
            }
            set
            {
                if (IsExpanded == value)
                {
                    return;
                }

                if (_serializedProperty != null)
                {
                    _serializedProperty.isExpanded = value;
                }
                else if (_isExpandedPrefsKey != null)
                {
                    EditorPrefs.SetBool(_isExpandedPrefsKey, value);
                }
            }
        }

        [PublicAPI]
        [CanBeNull]
        public Type ValueType { get; private set; }

        public bool IsValueMixed { get; private set; }

        [PublicAPI]
        public TriPropertyType PropertyType { get; }

        [PublicAPI]
        public IReadOnlyList<TriProperty> ChildrenProperties =>
            PropertyType == TriPropertyType.Generic || PropertyType == TriPropertyType.Reference
                ? _childrenProperties
                : throw new InvalidOperationException("Cannot read ChildrenProperties for " + PropertyType);

        [PublicAPI]
        public IReadOnlyList<TriProperty> ArrayElementProperties => PropertyType == TriPropertyType.Array
            ? _childrenProperties
            : throw new InvalidOperationException("Cannot read ArrayElementProperties for " + PropertyType);

        [PublicAPI]
        public TriPropertyTree PropertyTree { get; }

        [PublicAPI]
        [CanBeNull]
        public object Value { get; private set; }

        public object GetValue(int targetIndex)
        {
            return _definition.GetValue(this, targetIndex);
        }

        [PublicAPI]
        public void SetValue(object value)
        {
            ModifyAndRecordForUndo(targetIndex => SetValueRecursive(this, value, targetIndex));
        }

        [PublicAPI]
        public void SetValues(Func<int, object> getValue)
        {
            ModifyAndRecordForUndo(targetIndex =>
            {
                var value = getValue.Invoke(targetIndex);
                SetValueRecursive(this, value, targetIndex);
            });
        }

        public void ModifyAndRecordForUndo(Action<int> call)
        {
            // save any pending changes
            PropertyTree.ApplySerializedObjectModifiedProperties();

            // record object state for undp
            Undo.RegisterCompleteObjectUndo(PropertyTree.TargetObjects, "Inspector");
            Undo.FlushUndoRecordObjects();

            // set value for all targets
            for (var targetIndex = 0; targetIndex < PropertyTree.TargetObjects.Length; targetIndex++)
            {
                call.Invoke(targetIndex);

                EditorUtility.SetDirty(PropertyTree.TargetObjects[targetIndex]);
            }

            // actualize
            PropertyTree.ForceUpdateSerializedObject();
            Update();

            NotifyValueChanged();
        }

        public void NotifyValueChanged()
        {
            ((ITriPropertyParent) this).NotifyValueChanged(this);
        }

        void ITriPropertyParent.NotifyValueChanged(TriProperty property)
        {
            if (_definition.OnValueChanged != null)
            {
                _serializedProperty?.serializedObject.ApplyModifiedProperties();

                for (var targetIndex = 0; targetIndex < PropertyTree.TargetObjects.Length; targetIndex++)
                {
                    _definition.OnValueChanged.InvokeForTarget(this, targetIndex);
                    EditorUtility.SetDirty(PropertyTree.TargetObjects[targetIndex]);
                }
            }

            _parent.NotifyValueChanged(property);
        }

        internal void Update()
        {
            ReadValue(this, out var newValue, out var newValueIsMixed);

            var newValueType = FieldType.IsValueType ? FieldType
                : ReferenceEquals(Value, newValue) ? ValueType
                : newValue?.GetType();
            var valueTypeChanged = ValueType != newValueType;

            Value = newValue;
            ValueType = newValueType;
            IsValueMixed = newValueIsMixed;

            switch (PropertyType)
            {
                case TriPropertyType.Generic:
                case TriPropertyType.Reference:
                    if (_childrenProperties == null || valueTypeChanged)
                    {
                        if (_childrenProperties == null)
                        {
                            _childrenProperties = new List<TriProperty>();
                        }

                        _childrenProperties.Clear();

                        var selfType = PropertyType == TriPropertyType.Reference ? ValueType : FieldType;
                        if (selfType != null)
                        {
                            var properties = TriTypeDefinition.GetCached(selfType).Properties;
                            for (var index = 0; index < properties.Count; index++)
                            {
                                var childDefinition = properties[index];
                                var childSerializedProperty =
                                    _serializedProperty?.FindPropertyRelative(childDefinition.Name);
                                var childProperty = new TriProperty(PropertyTree, this,
                                    childDefinition, index, childSerializedProperty);

                                _childrenProperties.Add(childProperty);
                            }
                        }
                    }

                    break;

                case TriPropertyType.Array:
                    if (_childrenProperties == null)
                    {
                        _childrenProperties = new List<TriProperty>();
                    }

                    var listSize = ((IList) newValue)?.Count ?? 0;

                    while (_childrenProperties.Count < listSize)
                    {
                        var index = _childrenProperties.Count;
                        var elementDefinition = _definition.ArrayElementDefinition;
                        var elementSerializedReference = _serializedProperty?.GetArrayElementAtIndex(index);

                        var elementProperty = new TriProperty(PropertyTree, this,
                            elementDefinition, index, elementSerializedReference);

                        _childrenProperties.Add(elementProperty);
                    }

                    while (_childrenProperties.Count > listSize)
                    {
                        _childrenProperties.RemoveAt(_childrenProperties.Count - 1);
                    }

                    break;
            }

            if (_childrenProperties != null)
            {
                foreach (var childrenProperty in _childrenProperties)
                {
                    childrenProperty.Update();
                }
            }
        }

        internal void RunValidation()
        {
            if (HasValidators)
            {
                _validationResults = _definition.Validators
                    .Select(it => it.Validate(this))
                    .Where(it => !it.IsValid)
                    .ToList();
            }

            if (_childrenProperties != null)
            {
                foreach (var childrenProperty in _childrenProperties)
                {
                    childrenProperty.RunValidation();
                }
            }
        }

        [PublicAPI]
        public bool TryGetSerializedProperty(out SerializedProperty serializedProperty)
        {
            serializedProperty = _serializedProperty;
            return serializedProperty != null;
        }

        [PublicAPI]
        public bool TryGetAttribute<TAttribute>(out TAttribute attribute)
            where TAttribute : Attribute
        {
            if (ValueType != null)
            {
                foreach (var attr in TriReflectionUtilities.GetAttributesCached(ValueType))
                {
                    if (attr is TAttribute typedAttr)
                    {
                        attribute = typedAttr;
                        return true;
                    }
                }
            }

            foreach (var attr in _definition.Attributes)
            {
                if (attr is TAttribute typedAttr)
                {
                    attribute = typedAttr;
                    return true;
                }
            }

            attribute = null;
            return false;
        }

        public static bool AreValuesEqual(Type type, object a, object b)
        {
            if (type == typeof(string))
            {
                return string.Equals((string) a, (string) b);
            }

            if (type.IsValueType || type == typeof(string))
            {
                return a.Equals(b);
            }

            return ReferenceEquals(a, b);
        }

        private static void SetValueRecursive(TriProperty property, object value, int targetIndex)
        {
            // for value types we must recursively set all parent objects
            // because we cannot directly modify structs
            // but we can re-set entire parent value
            while (property._definition.SetValue(property, value, targetIndex, out var parentValue) &&
                   property.Parent is TriProperty parentProperty &&
                   parentProperty.ValueType != null && parentProperty.ValueType.IsValueType)
            {
                property = parentProperty;
                value = parentValue;
            }
        }

        private static void ReadValue(TriProperty property, out object newValue, out bool isMixed)
        {
            newValue = property.GetValue(0);

            if (property.PropertyTree.TargetObjects.Length == 1)
            {
                isMixed = false;
                return;
            }

            switch (property.PropertyType)
            {
                case TriPropertyType.Array:
                {
                    var list = (IList) newValue;
                    for (var i = 1; i < property.PropertyTree.TargetObjects.Length; i++)
                    {
                        if (list == null)
                        {
                            break;
                        }

                        var otherList = (IList) property.GetValue(i);
                        if (otherList == null || otherList.Count < list.Count)
                        {
                            newValue = list = otherList;
                        }
                    }

                    isMixed = true;
                    return;
                }
                case TriPropertyType.Reference:
                {
                    for (var i = 1; i < property.PropertyTree.TargetObjects.Length; i++)
                    {
                        var otherValue = property.GetValue(i);

                        if (newValue?.GetType() != otherValue?.GetType())
                        {
                            isMixed = true;
                            newValue = null;
                            return;
                        }
                    }

                    isMixed = false;
                    return;
                }
                case TriPropertyType.Generic:
                {
                    isMixed = false;
                    return;
                }
                case TriPropertyType.Primitive:
                {
                    for (var i = 1; i < property.PropertyTree.TargetObjects.Length; i++)
                    {
                        var otherValue = property.GetValue(i);
                        if (!AreValuesEqual(property.FieldType, otherValue, newValue))
                        {
                            isMixed = true;
                            return;
                        }
                    }

                    isMixed = false;
                    return;
                }

                default:
                {
                    Debug.LogError($"Unexpected property type: {property.PropertyType}");
                    isMixed = true;
                    return;
                }
            }
        }

        private static TriPropertyType GetPropertyType(TriProperty property)
        {
            if (property._serializedProperty != null)
            {
                if (property._serializedProperty.isArray &&
                    property._serializedProperty.propertyType != SerializedPropertyType.String)
                {
                    return TriPropertyType.Array;
                }

                if (property._serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                {
                    return TriPropertyType.Reference;
                }

                if (property._serializedProperty.propertyType == SerializedPropertyType.Generic)
                {
                    return TriPropertyType.Generic;
                }

                return TriPropertyType.Primitive;
            }

            if (property._definition.FieldType.IsPrimitive ||
                property._definition.FieldType == typeof(string))
            {
                return TriPropertyType.Primitive;
            }

            if (property._definition.FieldType.IsValueType)
            {
                return TriPropertyType.Generic;
            }

            if (property._definition.IsArray)
            {
                return TriPropertyType.Array;
            }

            return TriPropertyType.Reference;
        }
    }

    public interface ITriPropertyParent
    {
        object GetValue(int targetIndex);

        void NotifyValueChanged(TriProperty property);
    }

    public enum TriPropertyType
    {
        Array,
        Reference,
        Generic,
        Primitive,
    }
}