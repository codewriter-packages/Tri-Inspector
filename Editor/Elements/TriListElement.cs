using System;
using System.Collections;
using System.Linq;
using TriInspectorUnityInternalBridge;
using TriInspector.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TriInspector.Elements
{
    public class TriListElement : TriElement
    {
        private const int MinElementsForVirtualization = 25;

        private const float ListExtraWidth = 7f;
        private const float DraggableAreaExtraWidth = 14f;

        private readonly TriProperty _property;
        private readonly ReorderableList _reorderableListGui;
        private readonly ListPropertyOverrideContext _elementLabelOverride;
        private readonly bool _alwaysExpanded;
        private readonly bool _showElementLabels;
        private readonly bool _showAlternatingBackground;

        private float _lastContentWidth;
        private int? _lastInvisibleElement;
        private int? _lastVisibleElement;

        protected ReorderableList ListGui => _reorderableListGui;

        public TriListElement(TriProperty property)
        {
            property.TryGetAttribute(out ListDrawerSettingsAttribute settings);

            _property = property;
            _alwaysExpanded = settings?.AlwaysExpanded ?? false;
            _showElementLabels = settings?.ShowElementLabels ?? false;
            _showAlternatingBackground = settings?.ShowAlternatingBackground ?? true;
            _elementLabelOverride = new ListPropertyOverrideContext(_property, _showElementLabels);
            _reorderableListGui = new ReorderableList(null, _property.ArrayElementType)
            {
                showDefaultBackground = settings?.ShowDefaultBackground ?? true,
                draggable = settings?.Draggable ?? true,
                displayAdd = settings == null || !settings.HideAddButton,
                displayRemove = settings == null || !settings.HideRemoveButton,
                drawHeaderCallback = DrawHeaderCallback,
                elementHeightCallback = ElementHeightCallback,
                drawElementBackgroundCallback = DrawElementBackgroundCallback,
                drawElementCallback = DrawElementCallback,
                onAddCallback = AddElementCallback,
                onRemoveCallback = RemoveElementCallback,
                onReorderCallbackWithDetails = ReorderCallback,
            };

            if (!_reorderableListGui.displayAdd && !_reorderableListGui.displayRemove)
            {
                _reorderableListGui.footerHeight = 0f;
            }
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            return new ListViewTriElement(this);
        }

        public override bool Update()
        {
            var dirty = false;

            if (_property.TryGetSerializedProperty(out var serializedProperty) && serializedProperty.isArray)
            {
                _reorderableListGui.serializedProperty = serializedProperty;
            }
            else if (_property.Value != null)
            {
                _reorderableListGui.list = (IList) _property.Value;
            }
            else if (_reorderableListGui.list == null)
            {
                _reorderableListGui.list = (IList) (_property.FieldType.IsArray
                    ? Array.CreateInstance(_property.ArrayElementType, 0)
                    : Activator.CreateInstance(_property.FieldType));
            }

            if (_alwaysExpanded && !_property.IsExpanded)
            {
                _property.IsExpanded = true;
            }

            if (_property.IsExpanded)
            {
                dirty |= GenerateChildren();
            }
            else
            {
                dirty |= ClearChildren();
            }

            dirty |= base.Update();

            if (dirty)
            {
                ReorderableListProxy.ClearCacheRecursive(_reorderableListGui);
            }

            return dirty;
        }

        public override float GetHeight(float width)
        {
            if (!_property.IsExpanded)
            {
                return _reorderableListGui.headerHeight + 4f;
            }

            _lastContentWidth = width;

            return _reorderableListGui.GetHeight();
        }

        public override void OnGUI(Rect position)
        {
            if (!_property.IsExpanded)
            {
                _lastInvisibleElement = null;
                _lastVisibleElement = null;

                ReorderableListProxy.DoListHeader(_reorderableListGui, new Rect(position)
                {
                    yMax = position.yMax - 4,
                });
                return;
            }

            if (_reorderableListGui.count < MinElementsForVirtualization)
            {
                _lastInvisibleElement = null;
                _lastVisibleElement = null;
            }

            var labelWidthExtra = ListExtraWidth + DraggableAreaExtraWidth;

            using (TriGuiHelper.PushLabelWidth(EditorGUIUtility.labelWidth - labelWidthExtra))
            {
                _reorderableListGui.DoList(position);
            }
        }

        private void AddElementCallback(ReorderableList reorderableList)
        {
            AddElementCallback(reorderableList, null);
        }

        private void AddElementCallback(ReorderableList reorderableList, Object addedReferenceValue)
        {
            if (_property.TryGetSerializedProperty(out _))
            {
                ReorderableListProxy.DoAddButton(reorderableList, addedReferenceValue);
                _property.NotifyValueChanged();
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

        private void RemoveElementCallback(ReorderableList reorderableList)
        {
            if (_property.TryGetSerializedProperty(out _))
            {
                ReorderableListProxy.defaultBehaviours.DoRemoveButton(reorderableList);
                _property.NotifyValueChanged();
                return;
            }

            var template = CloneValue(_property);
            var ind = reorderableList.index;

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

        private void ReorderCallback(ReorderableList list, int oldIndex, int newIndex)
        {
            if (_property.TryGetSerializedProperty(out _))
            {
                _property.NotifyValueChanged();
                return;
            }

            var mainValue = _property.Value;

            _property.SetValues(targetIndex =>
            {
                var value = (IList) _property.GetValue(targetIndex);

                if (value == mainValue)
                {
                    return value;
                }

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

        private void SetArraySizeCallback(int arraySize)
        {
            if (arraySize < 0)
            {
                return;
            }

            if (_property.TryGetSerializedProperty(out var serializedProperty))
            {
                serializedProperty.arraySize = arraySize;
                _property.NotifyValueChanged();
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
                        var newElement = CreateDefaultElementValue(_property);
                        value.Add(newElement);
                    }
                }

                return value;
            });
        }

        private bool GenerateChildren()
        {
            var count = _reorderableListGui.count;

            if (ChildrenCount == count)
            {
                return false;
            }

            while (ChildrenCount < count)
            {
                var property = _property.ArrayElementProperties[ChildrenCount];
                AddChild(CreateItemElement(property));
            }

            while (ChildrenCount > count)
            {
                RemoveChildAt(ChildrenCount - 1);
            }

            return true;
        }

        private bool ClearChildren()
        {
            if (ChildrenCount == 0)
            {
                return false;
            }

            RemoveAllChildren();

            return true;
        }

        protected virtual TriElement CreateItemElement(TriProperty property)
        {
            return new TriPropertyElement(property, new TriPropertyElement.Props
            {
                forceInline = !_showElementLabels,
            });
        }

        private void DrawHeaderCallback(Rect rect)
        {
            var labelRect = new Rect(rect)
            {
                xMax = rect.xMax - 50,
            };
            var arraySizeRect = new Rect(rect)
            {
                xMin = labelRect.xMax,
            };

            if (_alwaysExpanded)
            {
                EditorGUI.LabelField(labelRect, _property.DisplayNameContent);
            }
            else
            {
                TriEditorGUI.Foldout(labelRect, _property);
            }

            EditorGUI.BeginChangeCheck();

            var newArraySize = EditorGUI.DelayedIntField(arraySizeRect, _reorderableListGui.count);

            if (EditorGUI.EndChangeCheck())
            {
                SetArraySizeCallback(newArraySize);
                return;
            }

            if (Event.current.type == EventType.DragUpdated && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDrop.objectReferences.All(obj => TryGetDragAndDropObject(obj, out _))
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;

                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.AcceptDrag();

                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (TryGetDragAndDropObject(obj, out var addedReferenceValue))
                    {
                        AddElementCallback(_reorderableListGui, addedReferenceValue);
                    }
                }

                Event.current.Use();
            }
        }
        
        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_lastInvisibleElement.HasValue && index + 1 < _lastInvisibleElement.Value ||
                _lastVisibleElement.HasValue && index - 1 > _lastVisibleElement.Value)
            {
                if (index != _reorderableListGui.index)
                {
                    return;
                }
            }

            if (_showAlternatingBackground && index % 2 != 0)
            {
                EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.15f));
            }

            ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused,
                _reorderableListGui.draggable);
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= ChildrenCount)
            {
                return;
            }

            if (_lastInvisibleElement.HasValue && index + 1 < _lastInvisibleElement.Value ||
                _lastVisibleElement.HasValue && index - 1 > _lastVisibleElement.Value)
            {
                if (index != _reorderableListGui.index)
                {
                    return;
                }
            }

            if (_reorderableListGui.count > MinElementsForVirtualization)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    var windowRect = GUIClipProxy.VisibleRect;
                    var rectInWindow = GUIClipProxy.UnClipToWindow(rect);

                    if (rectInWindow.yMax < 0)
                    {
                        _lastInvisibleElement = index;
                    } else if (_lastInvisibleElement == index)
                    {
                        _lastInvisibleElement = index / 2;
                        _lastVisibleElement = index / 2 + 1;
                        _property.PropertyTree.RequestRepaint();
                    }

                    if (rectInWindow.y < windowRect.height)
                    {
                        if (!_lastVisibleElement.HasValue || index > _lastVisibleElement.Value)
                        {
                            _lastVisibleElement = index;
                        }
                    }
                }
            }

            if (!_reorderableListGui.draggable)
            {
                rect.xMin += DraggableAreaExtraWidth;
            }

            GetChild(index).OnGUI(rect);
        }

        private float ElementHeightCallback(int index)
        {
            if (index >= ChildrenCount)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (_lastInvisibleElement.HasValue && index + 1 < _lastInvisibleElement.Value ||
                _lastVisibleElement.HasValue && index - 1 > _lastVisibleElement.Value)
            {
                if (index != _reorderableListGui.index)
                {
                    return Mathf.Max(EditorGUIUtility.singleLineHeight, GetChild(index).CachedHeight);
                }
            }

            return GetChild(index).GetHeight(_lastContentWidth);
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

        public sealed class ListViewTriElement : ListView, ITriElement
        {
            private readonly TriListElement _owner;

            public ListViewTriElement(TriListElement owner)
            {
                _owner = owner;

                var gui = owner._reorderableListGui;

                showFoldoutHeader = true;
                headerTitle = owner._property.DisplayName;
                showBoundCollectionSize = true;
                showAddRemoveFooter = gui.displayAdd || gui.displayRemove;
                reorderable = gui.draggable;
                reorderMode = ListViewReorderMode.Animated;
                showAlternatingRowBackgrounds = owner._showAlternatingBackground
                    ? AlternatingRowBackground.All
                    : AlternatingRowBackground.None;
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                selectionType = SelectionType.None;
                makeItem = () => new VisualElement();
                bindItem = BindListViewItem;
                unbindItem = (itemRoot, _) => itemRoot.Clear();

                if (owner._property.TryGetSerializedProperty(out var serializedProperty) && serializedProperty.isArray)
                {
                    this.BindProperty(serializedProperty);
                }
                else
                {
                    itemsSource = owner._property.Value as IList;
                    itemsAdded += _ => owner.AddElementCallback(gui, null);
                    itemsRemoved += indices =>
                    {
                        foreach (var index in indices.OrderByDescending(i => i))
                        {
                            gui.index = index;
                            owner.RemoveElementCallback(gui);
                        }
                    };
                    itemIndexChanged += (from, to) => owner.ReorderCallback(gui, from, to);
                }

                if (owner._alwaysExpanded)
                {
                    var foldout = this.Q<Foldout>();
                    if (foldout != null)
                    {
                        foldout.value = true;
                        foldout.RegisterValueChangedCallback(evt =>
                        {
                            if (!evt.newValue)
                            {
                                foldout.SetValueWithoutNotify(true);
                            }
                        });

                        var toggle = foldout.Q<Toggle>();
                        if (toggle != null)
                        {
                            toggle.SetEnabled(false);
                        }
                    }
                }

                RegisterListDragAndDrop();

                RegisterCallback<AttachToPanelEvent>(_ =>
                    owner._property.PropertyTree.AddPropertyOverride(owner._elementLabelOverride));
                RegisterCallback<DetachFromPanelEvent>(_ =>
                    owner._property.PropertyTree.RemovePropertyOverride(owner._elementLabelOverride));
            }

            public VisualElement CreateVisualElement(TriProperty property)
            {
                return this;
            }

            private void BindListViewItem(VisualElement itemRoot, int index)
            {
                itemRoot.Clear();

                var elementProperties = _owner._property.ArrayElementProperties;
                if (index < 0 || index >= elementProperties.Count)
                {
                    return;
                }

                var itemElement = _owner.CreateItemElement(elementProperties[index]);

                itemRoot.Add(itemElement.CreateVisualElement(elementProperties[index]));
            }

            private void RegisterListDragAndDrop()
            {
                RegisterCallback<DragUpdatedEvent>(evt =>
                {
                    DragAndDrop.visualMode =
                        DragAndDrop.objectReferences.All(obj => _owner.TryGetDragAndDropObject(obj, out _))
                            ? DragAndDropVisualMode.Copy
                            : DragAndDropVisualMode.Rejected;
                    evt.StopPropagation();
                });

                RegisterCallback<DragPerformEvent>(evt =>
                {
                    DragAndDrop.AcceptDrag();

                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (_owner.TryGetDragAndDropObject(obj, out var addedReferenceValue))
                        {
                            _owner.AddElementCallback(_owner._reorderableListGui, addedReferenceValue);
                        }
                    }

                    evt.StopPropagation();
                });
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