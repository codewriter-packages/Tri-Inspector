using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriDropdownElement : TriElement
    {
        private readonly TriProperty _property;
        private readonly Func<TriProperty, IEnumerable<ITriDropdownItem>> _valuesGetter;
        private readonly bool _useAdvancedDropdown;
        private readonly AdvancedDropdownState _dropdownState;

        private object _currentValue;
        private string _currentText;

        private bool _hasNextValue;
        private object _nextValue;

        public TriDropdownElement(TriProperty property, Func<TriProperty, IEnumerable<ITriDropdownItem>> valuesGetter,
            bool useAdvancedDropdown = true)
        {
            _property = property;
            _valuesGetter = valuesGetter;
            _useAdvancedDropdown = useAdvancedDropdown;
            _dropdownState = new AdvancedDropdownState();
        }

        public override float GetHeight(float width)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position)
        {
            if (_hasNextValue)
            {
                var nextValue = _nextValue;
                _hasNextValue = false;
                _nextValue = null;

                _property.SetValue(nextValue);
                GUI.changed = true;
            }

            if (!_property.Comparer.Equals(_currentValue, _property.Value))
            {
                _currentValue = _property.Value;
                _currentText = _valuesGetter.Invoke(_property)
                    .FirstOrDefault(it => _property.Comparer.Equals(it.Value, _property.Value))
                    ?.Text ?? (_property.Value?.ToString() ?? string.Empty);
            }

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);

            if (GUI.Button(position, _currentText, EditorStyles.popup))
            {
                if (_useAdvancedDropdown)
                {
                    ShowAdvancedDropdown(position);
                }
                else
                {
                    ShowDropdown(position);
                }
            }
        }

        private void ShowDropdown(Rect position)
        {
            var items = _valuesGetter.Invoke(_property);
            var menu = new GenericMenu();

            foreach (var item in items)
            {
                var isOn = _property.Comparer.Equals(item.Value, _property.Value);
                menu.AddItem(new GUIContent(item.Text), isOn, ChangeValue, item.Value);
            }

            menu.DropDown(position);
        }

        private void ShowAdvancedDropdown(Rect position)
        {
            var items = _valuesGetter.Invoke(_property);
            var dropdown = new TriAdvancedDropdown(_dropdownState, _property, items, ChangeValue);
            dropdown.Show(position);
        }

        private void ChangeValue(object v)
        {
            _nextValue = v;
            _hasNextValue = true;
            _property.PropertyTree.RequestRepaint();
        }

        private class TriAdvancedDropdown : AdvancedDropdown
        {
            private readonly TriProperty _property;
            private readonly IEnumerable<ITriDropdownItem> _items;
            private readonly Action<object> _onSelected;

            public TriAdvancedDropdown(AdvancedDropdownState state,
                TriProperty property,
                IEnumerable<ITriDropdownItem> items,
                Action<object> onSelected)
                : base(state)
            {
                _property = property;
                _items = items;
                _onSelected = onSelected;

                minimumSize = new Vector2(200, 300);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem(_property.DisplayName);

                foreach (var item in _items)
                {
                    var path = item.Text.Split('/');
                    var parent = root;

                    var isOn = _property.Comparer.Equals(item.Value, _property.Value);

                    for (var i = 0; i < path.Length; i++)
                    {
                        if (i == path.Length - 1)
                        {
                            parent.AddChild(new TriAdvancedDropdownItem(path[i], item.Value, isOn));
                        }
                        else
                        {
                            var child = parent.children.FirstOrDefault(c => c.name == path[i]);
                            if (child == null)
                            {
                                child = new AdvancedDropdownItem(path[i]);
                                parent.AddChild(child);
                            }

                            parent = child;
                        }
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is TriAdvancedDropdownItem customItem)
                {
                    _onSelected?.Invoke(customItem.Value);
                }
            }
        }

        private class TriAdvancedDropdownItem : AdvancedDropdownItem
        {
            private static readonly GUIContent Checkmark = EditorGUIUtility.IconContent("Checkmark") ?? GUIContent.none;

            public object Value { get; }

            public TriAdvancedDropdownItem(string name, object value, bool isOn) : base(name)
            {
                Value = value;

                if (isOn)
                {
                    icon = Checkmark.image as Texture2D;
                }
            }
        }
    }
}