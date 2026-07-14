using System;
using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(InlineButtonDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class InlineButtonDrawer : TriAttributeDrawer<InlineButtonAttribute>
    {
        private const float MinButtonWidth = 28f;
        private const float ButtonSpacing = 4f;
        private const float ButtonPadding = 12f;
        private const float MaxButtonWidthRatio = 0.25f;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (string.IsNullOrEmpty(Attribute.Name))
            {
                return "[InlineButton] method name cannot be empty";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return next.GetHeight(width);
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var buttonText = !string.IsNullOrEmpty(Attribute.ButtonLabel)
                ? Attribute.ButtonLabel
                : Attribute.Name;

            var buttonContent = new GUIContent(buttonText);
            var maxButtonWidth = position.width * MaxButtonWidthRatio;
            var buttonWidth = Attribute.ButtonWidth > 0f
                ? Attribute.ButtonWidth
                : Mathf.Clamp(GUI.skin.button.CalcSize(buttonContent).x + ButtonPadding, MinButtonWidth, maxButtonWidth);

            var propertyRect = new Rect(position)
            {
                width = position.width - buttonWidth - ButtonSpacing,
            };

            var buttonRect = new Rect(position)
            {
                x = position.x + position.width - buttonWidth,
                width = buttonWidth,
            };

            next.OnGUI(propertyRect);

            if (GUI.Button(buttonRect, buttonContent))
            {
                InvokeMethod(property);
            }
        }

        private void InvokeMethod(TriProperty property)
        {
            var methodName = Attribute.Name;

            property.ModifyAndRecordForUndo(targetIndex =>
            {
                try
                {
                    var parentValue = property.Parent.GetValue(targetIndex);
                    var targetType = parentValue?.GetType() ?? property.Parent.FieldType;

                    var methodInfo = targetType.GetMethod(
                        methodName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (methodInfo == null)
                    {
                        Debug.LogError($"[InlineButton] Method '{methodName}' not found on type '{targetType.Name}'");
                        return;
                    }

                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length > 0)
                    {
                        Debug.LogError($"[InlineButton] Method '{methodName}' must have no parameters");
                        return;
                    }

                    methodInfo.Invoke(parentValue, null);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }
    }
}