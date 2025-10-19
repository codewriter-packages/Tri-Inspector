using System;
using System.Collections.Generic;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(MinMaxSliderAttributeDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class MinMaxSliderAttributeDrawer : TriAttributeDrawer<MinMaxSliderAttribute>
    {
        private MinMaxSliderAttributeHelpers.SliderResolvers _resolvers;


        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvers = MinMaxSliderAttributeHelpers.Initialize(Attribute, propertyDefinition, out var errorResult);

            //MinMaxSliderAttributeValidator is expected to return an error result if initialization fails.
            if (errorResult.IsError)
            {
                return TriExtensionInitializationResult.Skip;
            }
            return TriExtensionInitializationResult.Ok;
        }
        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var (minLimit, maxLimit) = MinMaxSliderAttributeHelpers.GetLimits(property, Attribute, _resolvers);

            float xValue, yValue;
            if (property.FieldType == typeof(Vector2Int))
            {
                var val = (Vector2Int) property.Value;
                xValue = val.x;
                yValue = val.y;
            }
            else
            {
                var val = (Vector2) property.Value;
                xValue = val.x;
                yValue = val.y;
            }

            if (Attribute.AutoClamp)
            {
                float clampedX = xValue;
                float clampedY = yValue;

                if (clampedX > clampedY) (clampedX, clampedY) = (clampedY, clampedX);
                clampedX = Mathf.Clamp(clampedX, (float)minLimit, clampedY);
                clampedY = Mathf.Clamp(clampedY, clampedX, (float)maxLimit);

                const float epsilon = 1e-5f;
                if (Math.Abs(clampedX - xValue) > epsilon || Math.Abs(clampedY - yValue) > epsilon)
                {
                    xValue = clampedX;
                    yValue = clampedY;

                    MinMaxSliderAttributeHelpers.SetValue(property, xValue, yValue);
                }
            }

            var label = property.DisplayNameContent;
            var controlRect = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();
            TriEditorGUI.DrawMinMaxSlider(controlRect, ref xValue, ref yValue, (float)minLimit, (float)maxLimit);
            if (EditorGUI.EndChangeCheck())
            {
                MinMaxSliderAttributeHelpers.SetValue(property, xValue, yValue);
            }
        }
        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    internal static class MinMaxSliderAttributeHelpers
    {
        internal class SliderResolvers : SliderAttributeHelpers.SliderResolvers
        {
            internal SliderResolvers(ref HashSet<string> errors, TriPropertyDefinition propertyDefinition, MinMaxSliderAttribute attribute)
                : base(ref errors, propertyDefinition, attribute.MinMemberName, attribute.MaxMemberName, attribute.MinMaxMemberName)
            {
            }
        }
        public static SliderResolvers Initialize(MinMaxSliderAttribute attribute,
            TriPropertyDefinition propertyDefinition, out TriExtensionInitializationResult errorResult)
        {
            var errors = new HashSet<string>();

            if (propertyDefinition.FieldType != typeof(Vector2) && propertyDefinition.FieldType != typeof(Vector2Int))
            {
                errors.Add("[MinMaxRange] attribute can only be used on Vector2 or Vector2Int fields.");
            }

            var resolvers = new SliderResolvers(ref errors, propertyDefinition, attribute);

            if (errors.Count > 0)
            {
                errorResult = string.Join(Environment.NewLine, errors);
                return null;
            }

            errorResult = TriExtensionInitializationResult.Ok;
            return resolvers;
        }
        public static (double min, double max) GetLimits(TriProperty property, MinMaxSliderAttribute attribute, SliderResolvers resolvers)
        {
            return SliderAttributeHelpers.GetLimits(property, attribute.MinFixed, attribute.MaxFixed, resolvers);
        }
        public static void SetValue(TriProperty property, float x, float y)
        {
            if (property.ValueType == typeof(Vector2Int))
            {
                var newVec = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
                property.SetValue(newVec);
            }
            else
            {
                var newVec = new Vector2(x, y);
                property.SetValue(newVec);
            }
        }
    }
}