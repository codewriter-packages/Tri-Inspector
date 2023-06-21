using System;
using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;
using TriInspector.Resolvers;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

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

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new TriButtonElement(property, Attribute, _nameResolver);
        }

        private class TriButtonElement : TriHeaderGroupBaseElement
        {
            private readonly TriProperty _property;
            private readonly ButtonAttribute _attribute;
            private readonly ValueResolver<string> _nameResolver;
            private readonly object[] _invocationArgs;

            public TriButtonElement(TriProperty property, ButtonAttribute attribute,
                ValueResolver<string> nameResolver)
            {
                _property = property;
                _attribute = attribute;
                _nameResolver = nameResolver;

                var mi = property.TryGetMemberInfo(out var memberInfo)
                    ? (MethodInfo) memberInfo
                    : throw new Exception("TriButtonElement requires MethodInfo");

                var parameters = mi.GetParameters();

                _invocationArgs = new object[parameters.Length];

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

                    AddChild(new TriPropertyElement(pTriProperty));
                }
            }

            protected override float GetHeaderHeight(float width)
            {
                return GetButtonHeight();
            }

            protected override void DrawHeader(Rect position)
            {
                if (_invocationArgs.Length > 0)
                {
                    TriEditorGUI.DrawBox(position, TriEditorStyles.TabOnlyOne);
                }

                var name = _nameResolver.GetValue(_property);

                if (string.IsNullOrEmpty(name))
                {
                    name = _property.DisplayName;
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = _property.RawName;
                }

                var buttonRect = new Rect(position)
                {
                    height = GetButtonHeight(),
                };

                if (GUI.Button(buttonRect, name))
                {
                    InvokeButton(_property, _invocationArgs);
                }
            }

            private float GetButtonHeight()
            {
                return _attribute.ButtonSize != 0
                    ? _attribute.ButtonSize
                    : EditorGUIUtility.singleLineHeight;
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
    }
}