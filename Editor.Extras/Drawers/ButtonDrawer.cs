using System;
using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(ButtonDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class ButtonDrawer : TriAttributeDrawer<ButtonAttribute>
    {
        private ValueResolver<string> _nameResolver;

        public override void Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _nameResolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Name);
        }

        public override string CanDraw(TriProperty property)
        {
            var isValidMethod = property.MemberInfo is MethodInfo mi && mi.GetParameters().Length == 0;
            if (!isValidMethod)
            {
                return "[Button] valid only on methods without parameters";
            }

            if (_nameResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return null;
        }

        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var name = _nameResolver.GetValue(property, property.DisplayName);

            if (GUI.Button(position, name))
            {
                InvokeButton(property, Array.Empty<object>());
            }
        }

        private static void InvokeButton(TriProperty property, object[] parameters)
        {
            var methodInfo = (MethodInfo) property.MemberInfo;

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