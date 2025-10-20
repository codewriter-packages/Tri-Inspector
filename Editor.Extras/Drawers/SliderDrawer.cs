using System;
using System.Collections.Generic;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(SliderAttributeDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class SliderAttributeDrawer : TriAttributeDrawer<SliderAttribute>
    {
        private SliderAttributeHelpers.SliderResolvers _resolvers;


        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvers = SliderAttributeHelpers.Initialize(Attribute, propertyDefinition, out var errorResult);

            //SliderAttributeValidator is expected to return an error result if initialization fails.
            if (errorResult.IsError)
            {
                return TriExtensionInitializationResult.Skip;
            }
            return TriExtensionInitializationResult.Ok;
        }
        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var label = property.DisplayNameContent;
            double currentValue;
            try
            {
                currentValue = Convert.ToDouble(property.Value);
            }
            catch (Exception)
            {
                EditorGUI.LabelField(position, label.text, "Cannot convert value to a number.");
                return;
            }

            var (minLimit, maxLimit) = SliderAttributeHelpers.GetLimits(property, Attribute, _resolvers);

            if (Attribute.AutoClamp)
            {
                double clampedValue = Math.Clamp(currentValue, minLimit, maxLimit);
                const double epsilon = 1e-9;
                if (Math.Abs(clampedValue - currentValue) > epsilon)
                {
                    property.SetValue(Convert.ChangeType(clampedValue, property.ValueType));
                    currentValue = clampedValue;
                }
            }

            EditorGUI.BeginChangeCheck();
            float sliderValue = EditorGUI.Slider(position, label, (float) currentValue, (float) minLimit, (float) maxLimit);
            if (EditorGUI.EndChangeCheck())
            {
                var finalValue = Convert.ChangeType(sliderValue, property.ValueType);
                property.SetValue(finalValue);
            }
        }
        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    internal static class SliderAttributeHelpers
    {
        internal class SliderResolvers
        {
            public ValueResolver<float> minFloatResolver;
            public ValueResolver<int> minIntResolver;
            public ValueResolver<float> maxFloatResolver;
            public ValueResolver<int> maxIntResolver;
            public ValueResolver<Vector2> minMaxVector2Resolver;
            public ValueResolver<Vector2Int> minMaxVector2IntResolver;

            internal SliderResolvers(ref HashSet<string> errors, TriPropertyDefinition propertyDefinition, SliderAttribute attribute)
                : this(ref errors, propertyDefinition, attribute.MinMemberName, attribute.MaxMemberName, attribute.MinMaxMemberName)
            {
            }
            protected SliderResolvers(ref HashSet<string> errors, TriPropertyDefinition propertyDefinition, string minMemberName, string maxMemberName, string minMaxMemberName)
            {
                var resolverErrors = new HashSet<string>();

                bool hasMinMaxMember = !string.IsNullOrEmpty(minMaxMemberName);
                if (hasMinMaxMember)
                {
                    minMaxVector2Resolver = ValueResolver.Resolve<Vector2>(propertyDefinition, minMaxMemberName);
                    if (minMaxVector2Resolver.TryGetErrorString(out var vector2Error))
                    {
                        minMaxVector2Resolver = null;
                        minMaxVector2IntResolver = ValueResolver.Resolve<Vector2Int>(propertyDefinition, minMaxMemberName);
                        if (minMaxVector2IntResolver.TryGetErrorString(out var vector2IntError))
                        {
                            errors.Add(vector2Error);
                            errors.Add(vector2IntError);
                        }
                    }
                }

                bool hasMinMember = !string.IsNullOrEmpty(minMemberName);
                if (hasMinMember && !hasMinMaxMember)
                {
                    minFloatResolver = ValueResolver.Resolve<float>(propertyDefinition, minMemberName);
                    if (minFloatResolver.TryGetErrorString(out var floatError))
                    {
                        minFloatResolver = null;
                        minIntResolver = ValueResolver.Resolve<int>(propertyDefinition, minMemberName);
                        if (minIntResolver.TryGetErrorString(out var intError))
                        {
                            errors.Add(floatError);
                            errors.Add(intError);
                        }
                    }
                }

                bool hasMaxMember = !string.IsNullOrEmpty(maxMemberName);
                if (hasMaxMember && !hasMinMaxMember)
                {
                    maxFloatResolver = ValueResolver.Resolve<float>(propertyDefinition, maxMemberName);
                    if (maxFloatResolver.TryGetErrorString(out var floatError))
                    {
                        maxFloatResolver = null;
                        maxIntResolver = ValueResolver.Resolve<int>(propertyDefinition, maxMemberName);
                        if (maxIntResolver.TryGetErrorString(out var intError))
                        {
                            errors.Add(floatError);
                            errors.Add(intError);
                        }
                    }
                }
            }
        }
        private static bool IsNumericType(Type type)
        {
            if (type == null) return false;
            return typeof(IConvertible).IsAssignableFrom(type) &&
                   type != typeof(string) &&
                   type != typeof(bool) &&
                   type != typeof(char);
        }
        public static SliderResolvers Initialize(SliderAttribute attribute,
            TriPropertyDefinition propertyDefinition, out TriExtensionInitializationResult errorResult)
        {
            var errors = new HashSet<string>();

            if (!IsNumericType(propertyDefinition.FieldType))
            {
                errors.Add("[Slider] attribute can only be used on numeric fields (like int, float, double, etc.).");
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
        public static (double min, double max) GetLimits(TriProperty property, SliderAttribute attribute, SliderResolvers resolvers)
        {
            return GetLimits(property, attribute.MinFixed, attribute.MaxFixed, resolvers);
        }
        public static (double min, double max) GetLimits(TriProperty property, float minFixed, float maxFixed, SliderResolvers resolvers)
        {
            double minLimit = resolvers.minMaxVector2Resolver?.GetValue(property, Vector2.zero).x ??
                              resolvers.minMaxVector2IntResolver?.GetValue(property, Vector2Int.zero).x ??
                              resolvers.minFloatResolver?.GetValue(property, minFixed) ??
                              resolvers.minIntResolver?.GetValue(property, (int) minFixed) ??
                              minFixed;

            double maxLimit = resolvers.minMaxVector2Resolver?.GetValue(property, Vector2.zero).y ??
                              resolvers.minMaxVector2IntResolver?.GetValue(property, Vector2Int.zero).y ??
                              resolvers.maxFloatResolver?.GetValue(property, maxFixed) ??
                              resolvers.maxIntResolver?.GetValue(property, (int) maxFixed) ??
                              maxFixed;

            if (minLimit > maxLimit) (minLimit, maxLimit) = (maxLimit, minLimit);

            if (property.FieldType == typeof(int) || property.FieldType == typeof(Vector2Int))
            {
                minLimit = Mathf.RoundToInt((float) minLimit);
                maxLimit = Mathf.RoundToInt((float) maxLimit);
            }

            return (minLimit, maxLimit);
        }
    }
}