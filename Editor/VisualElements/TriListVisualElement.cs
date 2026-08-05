using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using IList = System.Collections.IList;
using Object = UnityEngine.Object;

namespace TriInspector.VisualElements
{
    public class TriListVisualElement : ListView
    {
        private readonly TriProperty _property;
        private readonly bool _showElementLabels;
        private readonly bool _alwaysExpanded;
        private readonly SerializedProperty _serializedProperty;
        private ScrollView _scrollView;
        private bool _headerInitialized;

        public TriListVisualElement(TriProperty property)
        {
            AddToClassList(TriStyles.List);

            _property = property;

            property.TryGetAttribute(out ListDrawerSettingsAttribute settings);

            _showElementLabels = settings?.ShowElementLabels ?? false;

            _alwaysExpanded = settings?.AlwaysExpanded ?? false;
            var showAlternatingBackground = settings?.ShowAlternatingBackground ?? true;
            var elementLabelOverride = new ListPropertyOverrideContext(_property, _showElementLabels);

            allowAdd = settings == null || !settings.HideAddButton;
            allowRemove = settings == null || !settings.HideRemoveButton;

            showFoldoutHeader = false;
            showBoundCollectionSize = false;
            showAddRemoveFooter = false;

            reorderable = settings?.Draggable ?? true;
            reorderMode = ListViewReorderMode.Animated;
            showAlternatingRowBackgrounds = showAlternatingBackground
                ? AlternatingRowBackground.All
                : AlternatingRowBackground.None;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            selectionType = SelectionType.None;

            makeItem = () => new VisualElement();
            bindItem = BindListViewItem;
            unbindItem = (itemRoot, _) => itemRoot.Clear();

            if (property.TryGetSerializedProperty(out var serializedProperty) && serializedProperty.isArray)
            {
                _serializedProperty = serializedProperty;
                this.BindProperty(serializedProperty);
            }
            else
            {
                itemsSource = CreateListReadProxy(property);
                itemsAdded += indices =>
                {
                    foreach (var index in indices)
                    {
                        AddElementCallback(null);
                    }
                };
                itemsRemoved += indices =>
                {
                    foreach (var index in indices.OrderByDescending(i => i))
                    {
                        RemoveElementCallback(index);
                    }
                };
                itemIndexChanged += (from, to) => ReorderCallback(from, to);
            }

            void OnPropertyValueChanged(TriProperty obj)
            {
                if (!property.TryGetSerializedProperty(out _))
                {
                    itemsSource = CreateListReadProxy(property);
                }
            }

            // Deferred to attach: assigning makeHeader invokes CreateHeader synchronously, and CreateHeader
            // is virtual — running it now (mid-construction) would hit uninitialized subclass state.
            RegisterCallback<AttachToPanelEvent>(OnAttachInitializeHeader);

            RegisterListDragAndDrop();

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                _property.ValueChanged += OnPropertyValueChanged;
                _property.PropertyTree.AddPropertyOverride(elementLabelOverride);
            });

            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                _property.ValueChanged -= OnPropertyValueChanged;
                _property.PropertyTree.RemovePropertyOverride(elementLabelOverride);
            });
        }

        private void BindListViewItem(VisualElement itemRoot, int index)
        {
            itemRoot.Clear();

            var elements = _property.ArrayElementProperties;
            if (index < 0 || index >= elements.Count)
            {
                return;
            }

            var row = new TriLabelWidthContextVisualElement(null);
            row.AddToClassList(TriStyles.ListElement);

            var itemElement = CreateItemElement(elements[index]);
            itemElement.AddToClassList(TriStyles.ListElementContent);
            row.Add(itemElement);

            if (allowRemove)
            {
                var elementIndex = index;
                var removeButton = new Button(() => RemoveElementCallback(elementIndex)) {text = "✕"};
                removeButton.AddToClassList(TriStyles.ListElementRemove);
                removeButton.RemoveFromClassList("unity-button");
                row.Add(removeButton);
            }

            itemRoot.Add(row);
        }

        protected virtual VisualElement CreateItemElement(TriProperty property)
        {
            return new TriPropertyVisualElement(property, new TriPropertyVisualElement.Props
            {
                forceInline = !_showElementLabels,
            });
        }

        private void OnAttachInitializeHeader(AttachToPanelEvent _)
        {
            if (_headerInitialized)
            {
                return;
            }

            _headerInitialized = true;

            makeHeader = CreateHeader;
            _scrollView = this.Q<ScrollView>();
            SetExpanded(_alwaysExpanded || _property.IsExpanded);
        }

        protected virtual VisualElement CreateHeader()
        {
            return new TriListHeaderVisualElement(_property, new TriListHeaderVisualElement.Props
            {
                expanded = _alwaysExpanded || _property.IsExpanded,
                collapsible = !_alwaysExpanded,
                allowAdd = allowAdd,
                getCount = () => ItemCount,
                setCount = SetArraySizeCallback,
                addItem = () => AddElementCallback(null),
                expandedChanged = SetExpanded,
            });
        }

        protected virtual void SetExpanded(bool expanded)
        {
            _property.IsExpanded = expanded;

            if (_scrollView != null)
            {
                _scrollView.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private int ItemCount => _serializedProperty != null
            ? _serializedProperty.arraySize
            : GetValueListLength(_property);

        private void RegisterListDragAndDrop()
        {
            RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDrop.objectReferences.All(obj => TryGetDragAndDropObject(obj, out _))
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
            });

            RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();

                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (TryGetDragAndDropObject(obj, out var addedReferenceValue))
                    {
                        AddElementCallback(addedReferenceValue);
                    }
                }

                evt.StopPropagation();
            });
        }

        private void AddElementCallback(Object addedReferenceValue)
        {
            if (_serializedProperty != null)
            {
                var index = _serializedProperty.arraySize;
                _serializedProperty.arraySize++;

                if (addedReferenceValue != null)
                {
                    _serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = addedReferenceValue;
                }

                _serializedProperty.serializedObject.ApplyModifiedProperties();
                _property.RefreshValue();
                return;
            }

            var template = CloneValue(_property);

            _property.SetValues(targetIndex =>
            {
                var value = (IList) _property.GetValue(targetIndex);

                if (_property.FieldType.IsArray)
                {
                    var array = Array.CreateInstance(_property.ArrayElementType, template.Length + 1);
                    Array.Copy(template, array, template.Length);

                    if (addedReferenceValue != null)
                    {
                        array.SetValue(addedReferenceValue, array.Length - 1);
                    }

                    value = array;
                }
                else
                {
                    if (value == null)
                    {
                        value = (IList) Activator.CreateInstance(_property.FieldType);
                    }

                    var newElement = addedReferenceValue != null
                        ? addedReferenceValue
                        : CreateDefaultElementValue(_property);

                    value.Add(newElement);
                }

                return value;
            });
        }

        private void RemoveElementCallback(int ind)
        {
            if (_serializedProperty != null)
            {
                if (_serializedProperty.arraySize == 0)
                {
                    return;
                }

                ind = Mathf.Clamp(ind, 0, _serializedProperty.arraySize - 1);

                var element = _serializedProperty.GetArrayElementAtIndex(ind);

                // Object-reference elements need an extra pass: the first delete only nulls them.
                if (element.propertyType == SerializedPropertyType.ObjectReference &&
                    element.objectReferenceValue != null)
                {
                    element.objectReferenceValue = null;
                }

                _serializedProperty.DeleteArrayElementAtIndex(ind);
                _serializedProperty.serializedObject.ApplyModifiedProperties();
                _property.RefreshValue();
                return;
            }

            var template = CloneValue(_property);

            _property.SetValues(targetIndex =>
            {
                var value = (IList) _property.GetValue(targetIndex);

                if (_property.FieldType.IsArray)
                {
                    var array = Array.CreateInstance(_property.ArrayElementType, template.Length - 1);
                    Array.Copy(template, 0, array, 0, ind);
                    Array.Copy(template, ind + 1, array, ind, array.Length - ind);
                    value = array;
                }
                else
                {
                    value?.RemoveAt(ind);
                }

                return value;
            });
        }

        private void SetArraySizeCallback(int arraySize)
        {
            if (arraySize < 0)
            {
                return;
            }

            if (_serializedProperty != null)
            {
                _serializedProperty.arraySize = arraySize;
                _serializedProperty.serializedObject.ApplyModifiedProperties();
                _property.RefreshValue();
                return;
            }

            var template = CloneValue(_property);

            _property.SetValues(targetIndex =>
            {
                var value = (IList) _property.GetValue(targetIndex);

                if (_property.FieldType.IsArray)
                {
                    var array = Array.CreateInstance(_property.ArrayElementType, arraySize);
                    Array.Copy(template, array, Math.Min(arraySize, template.Length));

                    value = array;
                }
                else
                {
                    if (value == null)
                    {
                        value = (IList) Activator.CreateInstance(_property.FieldType);
                    }

                    while (value.Count > arraySize)
                    {
                        value.RemoveAt(value.Count - 1);
                    }

                    while (value.Count < arraySize)
                    {
                        value.Add(CreateDefaultElementValue(_property));
                    }
                }

                return value;
            });
        }

        private void ReorderCallback(int oldIndex, int newIndex)
        {
            _property.SetValues(targetIndex =>
            {
                var value = (IList) _property.GetValue(targetIndex);

                var element = value[oldIndex];
                for (var index = 0; index < value.Count - 1; ++index)
                {
                    if (index >= oldIndex)
                    {
                        value[index] = value[index + 1];
                    }
                }

                for (var index = value.Count - 1; index > 0; --index)
                {
                    if (index > newIndex)
                    {
                        value[index] = value[index - 1];
                    }
                }

                value[newIndex] = element;

                return value;
            });
        }

        private bool TryGetDragAndDropObject(Object obj, out Object result)
        {
            if (obj == null)
            {
                result = null;
                return false;
            }

            var elementType = _property.ArrayElementType;
            var objType = obj.GetType();

            if (elementType == objType || elementType.IsAssignableFrom(objType))
            {
                result = obj;
                return true;
            }

            if (obj is GameObject go && typeof(Component).IsAssignableFrom(elementType) &&
                go.TryGetComponent(elementType, out var component))
            {
                result = component;
                return true;
            }

            result = null;
            return false;
        }

        private static object CreateDefaultElementValue(TriProperty property)
        {
            var canActivate = property.ArrayElementType.IsValueType ||
                              property.ArrayElementType.GetConstructor(Type.EmptyTypes) != null;

            return canActivate ? Activator.CreateInstance(property.ArrayElementType) : null;
        }

        private static Array CloneValue(TriProperty property)
        {
            var list = (IList) property.Value;
            var template = Array.CreateInstance(property.ArrayElementType, list?.Count ?? 0);
            list?.CopyTo(template, 0);
            return template;
        }

        private static IList CreateListReadProxy(TriProperty property)
        {
            return property.Value is IList list ? new ListReadProxy(list) : null;
        }

        private static int GetValueListLength(TriProperty property)
        {
            return property.Value is IList list ? list.Count : 0;
        }

        private class ListReadProxy : IList
        {
            private readonly IList _source;

            public ListReadProxy(IList source) => _source = source;

            public IEnumerator GetEnumerator() => _source.GetEnumerator();

            public void CopyTo(Array array, int index) => _source.CopyTo(array, index);

            public int Count => _source.Count;

            public bool IsSynchronized => _source.IsSynchronized;

            public object SyncRoot => _source.SyncRoot;

            public bool IsFixedSize => _source.IsFixedSize;

            public bool IsReadOnly => _source.IsReadOnly;

            public object this[int index]
            {
                get => _source[index];
                set { }
            }

            public int Add(object value) => -1;

            public void Clear()
            {
            }

            public bool Contains(object value) => _source.Contains(value);

            public int IndexOf(object value) => _source.IndexOf(value);

            public void Insert(int index, object value)
            {
            }

            public void Remove(object value)
            {
            }

            public void RemoveAt(int index)
            {
            }
        }

        private class ListPropertyOverrideContext : TriPropertyOverrideContext
        {
            private readonly TriProperty _listProperty;
            private readonly bool _showElementLabels;
            private readonly GUIContent _noneLabel = GUIContent.none;

            public ListPropertyOverrideContext(TriProperty listProperty, bool showElementLabels)
            {
                _listProperty = listProperty;
                _showElementLabels = showElementLabels;
            }

            public override bool TryGetDisplayName(TriProperty property, out GUIContent displayName)
            {
                if (!_showElementLabels && property.Parent == _listProperty)
                {
                    displayName = _noneLabel;
                    return true;
                }

                displayName = default;
                return false;
            }
        }
    }
}