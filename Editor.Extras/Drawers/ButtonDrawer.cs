using System;
using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(ButtonDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class ButtonDrawer : TriAttributeDrawer<ButtonAttribute>
    {
        private ValueResolver<string> _nameResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var isValidMethod = propertyDefinition.TryGetMemberInfo(out var memberInfo) && memberInfo is MethodInfo;
            if (!isValidMethod)
            {
                return "[Button] valid only on methods";
            }

            _nameResolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Name);
            if (_nameResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriButtonVisualElement(property, Attribute, _nameResolver);
        }

        private class TriButtonVisualElement : VisualElement
        {
            private readonly TriProperty _property;
            private readonly ValueResolver<string> _nameResolver;
            private readonly object[] _invocationArgs;

            public TriButtonVisualElement(TriProperty property, ButtonAttribute attribute,
                ValueResolver<string> nameResolver)
            {
                _property = property;
                _nameResolver = nameResolver;

                var mi = property.TryGetMemberInfo(out var memberInfo)
                    ? (MethodInfo) memberInfo
                    : throw new Exception("TriButtonVisualElement requires MethodInfo");

                var parameters = mi.GetParameters();

                _invocationArgs = new object[parameters.Length];

                var button = new Button(OnButtonClicked)
                {
                    text = ResolveName(),
                };
                button.AddToClassList(Styles.Button);

                if (attribute.ButtonSize != 0)
                {
                    button.style.height = attribute.ButtonSize;
                }

                Add(button);

                if (parameters.Length == 0)
                {
                    Add(button);
                }
                else
                {
                    var box = new VisualElement();
                    box.AddToClassList(Styles.ButtonBox);
                    Add(box);

                    box.Add(button);

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var pIndex = i;
                        var pInfo = parameters[pIndex];

                        if (pInfo.HasDefaultValue)
                        {
                            _invocationArgs[pIndex] = pInfo.DefaultValue;
                        }

                        var pTriDefinition = TriPropertyDefinition.CreateForGetterSetter(
                            pIndex, pInfo.Name, pInfo.ParameterType,
                            ((self, targetIndex) => _invocationArgs[pIndex]),
                            ((self, targetIndex, value) => _invocationArgs[pIndex] = value));

                        var pTriProperty = new TriProperty(_property.PropertyTree, _property, pTriDefinition, null);

                        box.Add(new TriPropertyVisualElement(pTriProperty));
                    }
                }

                this.PeriodicRun(() => button.text = ResolveName());
            }

            private string ResolveName()
            {
                var buttonName = _nameResolver.GetValue(_property);

                if (string.IsNullOrEmpty(buttonName))
                {
                    buttonName = _property.DisplayName;
                }

                if (string.IsNullOrEmpty(buttonName))
                {
                    buttonName = _property.RawName;
                }

                return buttonName;
            }

            private void OnButtonClicked()
            {
                InvokeButton(_property, _invocationArgs);
            }
        }

        private static void InvokeButton(TriProperty property, object[] parameters)
        {
            if (property.TryGetMemberInfo(out var memberInfo) && memberInfo is MethodInfo methodInfo)
            {
                property.ModifyAndRecordForUndo(targetIndex =>
                {
                    try
                    {
                        var parentValue = property.Parent.GetValue(targetIndex);
                        methodInfo.Invoke(parentValue, parameters);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
            }
        }

        private static class Styles
        {
            public const string Button = "tri-button";
            public const string ButtonBox = "tri-button__box";
        }
    }
}