using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(DropdownDrawer<>), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class DropdownDrawer<T> : TriAttributeDrawer<DropdownAttribute>
    {
        private ValueResolver<IEnumerable<TriDropdownItem<T>>> _itemsResolver;
        private ValueResolver<IEnumerable<T>> _valuesResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _valuesResolver = ValueResolver.Resolve<IEnumerable<T>>(propertyDefinition, Attribute.Values);

            if (_valuesResolver.TryGetErrorString(out _))
            {
                _itemsResolver =
                    ValueResolver.Resolve<IEnumerable<TriDropdownItem<T>>>(propertyDefinition, Attribute.Values);

                if (_itemsResolver.TryGetErrorString(out var itemResolverError))
                {
                    return itemResolverError;
                }
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new DropdownElement(property, GetDropdownItems);
        }

        private IEnumerable<ITriDropdownItem> GetDropdownItems(TriProperty property)
        {
            if (_valuesResolver != null)
            {
                var values = _valuesResolver.GetValue(property, Enumerable.Empty<T>());

                foreach (var value in values)
                {
                    yield return new TriDropdownItem {Text = $"{value}", Value = value,};
                }
            }

            if (_itemsResolver != null)
            {
                var values = _itemsResolver.GetValue(property, Enumerable.Empty<TriDropdownItem<T>>());

                foreach (var value in values)
                {
                    yield return value;
                }
            }
        }

        private class DropdownElement : TriElement
        {
            private readonly TriProperty _property;
            private readonly Func<TriProperty, IEnumerable<ITriDropdownItem>> _valuesGetter;

            private string _currentText;

            public DropdownElement(TriProperty property, Func<TriProperty, IEnumerable<ITriDropdownItem>> valuesGetter)
            {
                _property = property;
                _valuesGetter = valuesGetter;
            }

            protected override void OnAttachToPanel()
            {
                base.OnAttachToPanel();

                _property.ValueChanged += OnValueChanged;

                RefreshCurrentText();
            }

            protected override void OnDetachFromPanel()
            {
                _property.ValueChanged -= OnValueChanged;

                base.OnDetachFromPanel();
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);

                if (GUI.Button(position, _currentText, EditorStyles.popup))
                {
                    ShowDropdown(position);
                }
            }

            private void OnValueChanged(TriProperty property)
            {
                RefreshCurrentText();
            }

            private void RefreshCurrentText()
            {
                var items = _valuesGetter.Invoke(_property);

                _currentText = items
                    .FirstOrDefault(it => _property.Comparer.Equals(it.Value, _property.Value))
                    ?.Text ?? "";
            }

            private void ShowDropdown(Rect position)
            {
                var items = _valuesGetter.Invoke(_property);
                var menu = new GenericMenu();

                foreach (var item in items)
                {
                    var isOn = _property.Comparer.Equals(item.Value, _property.Value);
                    menu.AddItem(new GUIContent(item.Text), isOn, _property.SetValue, item.Value);
                }

                menu.DropDown(position);
            }
        }
    }
}