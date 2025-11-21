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
                    var key = $"TriInspector.{_property.PropertyTree.TargetObjectType}.{_property.RawName}.{pInfo.Name}";

                    if (pInfo.HasDefaultValue)
                    {
                        _invocationArgs[pIndex] = GetValue(key, pInfo.ParameterType, pInfo.DefaultValue);
                    }

                    var pTriDefinition = TriPropertyDefinition.CreateForGetterSetter(
                        pIndex, pInfo.Name, pInfo.ParameterType,
                        ((self, targetIndex) => _invocationArgs[pIndex]),
                        ((self, targetIndex, value) =>
                        {
                            SetValue(key, pInfo.ParameterType, value);
                            return _invocationArgs[pIndex] = value;
                        }));

                    var pTriProperty = new TriProperty(_property.PropertyTree, _property, pTriDefinition, null);

                    AddChild(new TriPropertyElement(pTriProperty));

                    static object GetValue(string key, Type type, object defaultValue)
                    {
                        return type switch
                        {
                            Type t when t == typeof(string) => EditorPrefs.GetString(key, (string) defaultValue),
                            Type t when t == typeof(int) => EditorPrefs.GetInt(key, (int) defaultValue),
                            Type t when t == typeof(bool) => EditorPrefs.GetBool(key, (bool) defaultValue),
                            Type t when t == typeof(float) => EditorPrefs.GetFloat(key, (float) defaultValue),
                            _ => defaultValue,
                        };
                    }
                    static void SetValue(string key, Type type, object value)
                    {
                        if (EditorApplication.isPlaying)
                            return;
                        switch (type)
                        {
                            case Type t when t == typeof(string):
                                EditorPrefs.SetString(key, (string) value);
                                break;
                            case Type t when t == typeof(int):
                                EditorPrefs.SetInt(key, (int) value);
                                break;
                            case Type t when t == typeof(bool):
                                EditorPrefs.SetBool(key, (bool) value);
                                break;
                            case Type t when t == typeof(float):
                                EditorPrefs.SetFloat(key, (float) value);
                                break;
                        }
                    }
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