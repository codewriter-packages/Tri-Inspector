using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriListHeaderVisualElement : VisualElement
    {
        public struct Props
        {
            public bool expanded;
            public bool collapsible;
            public bool allowAdd;
            public Func<int> getCount;
            public Action<int> setCount;
            public Action addItem;
            public Action<bool> expandedChanged;
        }

        private readonly IntegerField _sizeField;
        private readonly Func<int> _getCount;
        private int _displayedCount = -1;

        public TriListHeaderVisualElement(TriProperty property, Props props)
        {
            _getCount = props.getCount;

            AddToClassList(TriStyles.ListHeader);

            var foldout = new Foldout
            {
                value = props.expanded,
            };
            foldout.SetAcceptClicksIfDisabled(true);
            foldout.AutoSyncLabelFromProperty(property);
            foldout.AddToClassList(TriStyles.ListHeaderFoldout);
            foldout.EnableInClassList(TriStyles.ListHeaderFoldoutCollapsible, props.collapsible);
            foldout.EnableInClassList(TriStyles.ListHeaderFoldoutNonCollapsible, !props.collapsible);
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.target == foldout)
                {
                    props.expandedChanged?.Invoke(evt.newValue);
                }
            });

            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                foldout.BindProperty(serializedProperty);
            }

            if (!props.collapsible)
            {
                foldout.SetValueWithoutNotify(true);
                foldout.toggleOnLabelClick = false;
            }

            Add(foldout);

            _sizeField = new IntegerField {isDelayed = true};
            _sizeField.AddToClassList(TriStyles.ListHeaderSize);
            _sizeField.RegisterValueChangedCallback(evt =>
            {
                var target = Mathf.Max(0, evt.newValue);
                if (target != evt.newValue)
                {
                    _sizeField.SetValueWithoutNotify(target);
                }

                props.setCount?.Invoke(target);
            });
            Add(_sizeField);

            if (props.allowAdd)
            {
                var addButton = new Button(() => props.addItem?.Invoke()) {text = "+"};
                addButton.AddToClassList(TriStyles.ListHeaderButton);
                addButton.RemoveFromClassList("unity-button");
                Add(addButton);
            }

            RegisterCallback<AttachToPanelEvent>(_ => property.ValueChanged += OnPropertyValueChanged);
            RegisterCallback<DetachFromPanelEvent>(_ => property.ValueChanged -= OnPropertyValueChanged);

            OnPropertyValueChanged(property);
        }

        private void OnPropertyValueChanged(TriProperty property)
        {
            if (_getCount == null)
            {
                return;
            }

            var count = _getCount();
            if (count == _displayedCount)
            {
                return;
            }

            _displayedCount = count;

            // Don't clobber the value while the user is editing it
            var focused = _sizeField.focusController?.focusedElement as VisualElement;
            var editing = focused != null && (focused == _sizeField || _sizeField.Contains(focused));
            if (!editing)
            {
                _sizeField.SetValueWithoutNotify(count);
            }
        }
    }
}