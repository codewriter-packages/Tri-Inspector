using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    public sealed class TriProperty : ITriPropertyParent
    {
        private static readonly IList EmptyList = new List<object>();

        private readonly TriPropertyDefinition _definition;
        private readonly int _propertyIndex;
        private readonly ITriPropertyParent _parent;
        [CanBeNull] private readonly SerializedProperty _serializedProperty;
        private List<TriProperty> _arrayElementProperties;
        private List<TriProperty> _childrenProperties;

        private GUIContent _displayNameBackingField;

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
        public GUIContent DisplayNameContent
        {
            get
            {
                if (_displayNameBackingField == null)
                {
                    if (TryGetAttribute(out HideLabelAttribute _))
                    {
                        _displayNameBackingField = GUIContent.none;
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
                else if (_displayNameBackingField != GUIContent.none)
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
        public Type FieldType => _definition.FieldType;

        [PublicAPI]
        public Type ArrayElementType => _definition.ArrayElementType;

        [PublicAPI]
        public bool IsArrayElement => _definition.IsArrayElement;

        public int IndexInArray => IsArrayElement
            ? _propertyIndex
            : throw new InvalidOperationException("Cannot read IndexInArray for !IsArrayElement");

        public IReadOnlyList<TriCustomDrawer> AllDrawers => _definition.Drawers;

        public ITriPropertyParent Parent => _parent;

        [PublicAPI]
        public bool IsExpanded
        {
            get
            {
                if (_serializedProperty != null)
                {
                    return _serializedProperty.isExpanded;
                }

                // add saves
                return true;
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
            }
        }

        [PublicAPI]
        [CanBeNull]
        public Type ValueType { get; private set; }

        [PublicAPI]
        public TriPropertyType PropertyType { get; }

        [PublicAPI]
        public IReadOnlyList<TriProperty> ChildrenProperties =>
            PropertyType == TriPropertyType.Generic || PropertyType == TriPropertyType.Reference
                ? _childrenProperties
                : throw new InvalidOperationException("Cannot read ChildrenProperties for " + PropertyType);

        [PublicAPI]
        public IReadOnlyList<TriProperty> ArrayElementProperties => PropertyType == TriPropertyType.Array
            ? _arrayElementProperties
            : throw new InvalidOperationException("Cannot read ArrayElementProperties for " + PropertyType);

        [PublicAPI]
        public TriPropertyTree PropertyTree { get; }

        public void ApplyChildValueModifications(int targetIndex)
        {
            var value = _definition.GetValue(this, targetIndex);
            _definition.SetValue(this, value, targetIndex);

            _parent.ApplyChildValueModifications(targetIndex);
        }

        [PublicAPI]
        [CanBeNull]
        public object Value { get; private set; }

        object ITriPropertyParent.GetValue(int targetIndex)
        {
            return _definition.GetValue(this, targetIndex);
        }

        [PublicAPI]
        public void SetValue(object value)
        {
            Undo.RegisterCompleteObjectUndo(PropertyTree.TargetObjects, "Inspector");
            Undo.FlushUndoRecordObjects();

            for (var i = 0; i < PropertyTree.TargetObjects.Length; i++)
            {
                _definition.SetValue(this, value, i);
            }

            _serializedProperty?.serializedObject.Update();

            if (_serializedProperty != null)
            {
                _serializedProperty.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                for (var i = 0; i < PropertyTree.TargetObjects.Length; i++)
                {
                    _parent.ApplyChildValueModifications(i);
                }
            }

            Update();
        }

        internal void Update()
        {
            var newValue = _definition.GetValue(this, 0);
            var valueChanged = !ReferenceEquals(Value, newValue);

            var newValueType = valueChanged ? newValue?.GetType() : ValueType;
            var valueTypeChanged = ValueType != newValueType;

            Value = newValue;
            ValueType = newValueType;

            switch (PropertyType)
            {
                case TriPropertyType.Generic:
                case TriPropertyType.Reference:
                    if (_childrenProperties == null || valueTypeChanged)
                    {
                        _childrenProperties ??= new List<TriProperty>();
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

                    foreach (var childrenProperty in _childrenProperties)
                    {
                        childrenProperty.Update();
                    }

                    break;

                case TriPropertyType.Array:
                    _arrayElementProperties ??= new List<TriProperty>();

                    var list = (IList) Value ?? EmptyList;

                    while (_arrayElementProperties.Count < list.Count)
                    {
                        var index = _arrayElementProperties.Count;
                        var elementDefinition = _definition.ArrayElementDefinition;
                        var elementSerializedReference = _serializedProperty?.GetArrayElementAtIndex(index);

                        var elementProperty = new TriProperty(PropertyTree, this,
                            elementDefinition, index, elementSerializedReference);

                        _arrayElementProperties.Add(elementProperty);
                    }

                    while (_arrayElementProperties.Count > list.Count)
                    {
                        _arrayElementProperties.RemoveAt(_arrayElementProperties.Count - 1);
                    }

                    for (var index = 0; index < _arrayElementProperties.Count; index++)
                    {
                        var arrayElementProperty = _arrayElementProperties[index];
                        arrayElementProperty.Update();
                    }

                    break;
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

        void ApplyChildValueModifications(int targetIndex);
    }

    public enum TriPropertyType
    {
        Array,
        Reference,
        Generic,
        Primitive,
    }
}