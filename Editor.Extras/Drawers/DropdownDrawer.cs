using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(DropdownDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class DropdownDrawer : TriAttributeDrawer<DropdownAttribute>
    {
        private DropdownResolver _resolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolver = DropdownResolver.Create(typeof(DropdownValueResolver<>), propertyDefinition, Attribute.Values);

            if (_resolver.TryGetErrorString(out var error))
            {
                _resolver = DropdownResolver.Create(typeof(DropdownItemResolver<>), propertyDefinition,
                    Attribute.Values);

                if (_resolver.TryGetErrorString(out error))
                {
                    return error;
                }
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new DropdownElement(property, _resolver);
        }

        private class DropdownElement : TriElement
        {
            private readonly TriProperty _property;
            private readonly DropdownResolver _resolver;

            private string _currentText;

            public DropdownElement(TriProperty property, DropdownResolver resolver)
            {
                _property = property;
                _resolver = resolver;
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
                var items = _resolver.GetDropdownItems(_property);

                _currentText = items
                    .FirstOrDefault(it => _resolver.EqualityComparer.Equals(it.Value, _property.Value))
                    ?.Text ?? "";
            }

            private void ShowDropdown(Rect position)
            {
                var items = _resolver.GetDropdownItems(_property);
                var menu = new GenericMenu();

                foreach (var item in items)
                {
                    var isOn = _resolver.EqualityComparer.Equals(item.Value, _property.Value);
                    menu.AddItem(new GUIContent(item.Text), isOn, _property.SetValue, item.Value);
                }

                menu.DropDown(position);
            }
        }

        private abstract class DropdownResolver
        {
            public abstract IEqualityComparer EqualityComparer { get; }

            public abstract void Initialize(TriPropertyDefinition propertyDefinition, string expression);

            public abstract bool TryGetErrorString(out string error);

            public abstract IEnumerable<ITriDropdownItem> GetDropdownItems(TriProperty property);

            public static DropdownResolver Create(Type resolverType,
                TriPropertyDefinition propertyDefinition, string expression)
            {
                var elementType = propertyDefinition.FieldType;
                var resolver = (DropdownResolver) Activator.CreateInstance(resolverType.MakeGenericType(elementType));
                resolver.Initialize(propertyDefinition, expression);
                return resolver;
            }
        }

        private class DropdownItemResolver<T> : DropdownResolver
        {
            private ValueResolver<IEnumerable<TriDropdownItem<T>>> _resolver;

            public override IEqualityComparer EqualityComparer { get; } = EqualityComparer<T>.Default;

            public override void Initialize(TriPropertyDefinition propertyDefinition, string expression)
            {
                _resolver = ValueResolver.Resolve<IEnumerable<TriDropdownItem<T>>>(propertyDefinition, expression);
            }

            public override bool TryGetErrorString(out string error)
            {
                return _resolver.TryGetErrorString(out error);
            }

            public override IEnumerable<ITriDropdownItem> GetDropdownItems(TriProperty property)
            {
                var values = _resolver.GetValue(property, Enumerable.Empty<TriDropdownItem<T>>());

                foreach (var value in values)
                {
                    yield return value;
                }
            }
        }

        private class DropdownValueResolver<T> : DropdownResolver
        {
            private ValueResolver<IEnumerable<T>> _resolver;

            public override IEqualityComparer EqualityComparer { get; } = EqualityComparer<T>.Default;

            public override void Initialize(TriPropertyDefinition propertyDefinition, string expression)
            {
                _resolver = ValueResolver.Resolve<IEnumerable<T>>(propertyDefinition, expression);
            }

            public override bool TryGetErrorString(out string error)
            {
                return _resolver.TryGetErrorString(out error);
            }

            public override IEnumerable<ITriDropdownItem> GetDropdownItems(TriProperty property)
            {
                var values = _resolver.GetValue(property, Enumerable.Empty<T>());

                foreach (var value in values)
                {
                    yield return new TriDropdownItem {Text = $"{value}", Value = value,};
                }
            }
        }
    }
}