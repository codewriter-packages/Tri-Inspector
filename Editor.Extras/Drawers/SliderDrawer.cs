using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly:
    RegisterTriAttributeDrawer(typeof(SliderAttributeDrawer), TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]

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

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriSlider(property, Attribute, _resolvers)
            {
                showInputField = true,
            };
        }

        private class TriSlider : Slider
        {
            private readonly TriProperty _property;
            private readonly SliderAttribute _attribute;
            private readonly SliderAttributeHelpers.SliderResolvers _resolvers;

            public TriSlider(TriProperty property, SliderAttribute attribute,
                SliderAttributeHelpers.SliderResolvers resolvers)
            {
                _property = property;
                _attribute = attribute;
                _resolvers = resolvers;

                this.BindTri(property, v => (float) Convert.ToDouble(v), v => ToPropertyTyped(v));

                this.PeriodicRun(RefreshFromProperty);
            }

            private object ToPropertyTyped(double v)
            {
                return Convert.ChangeType(v, _property.Definition.FieldType);
            }

            private void RefreshFromProperty()
            {
                var (minLimit, maxLimit) = SliderAttributeHelpers.GetLimits(_property, _attribute, _resolvers);

                this.SetClamped(false);

                lowValue = (float) minLimit;
                highValue = (float) maxLimit;
                showMixedValue = _property.IsValueMixed;

                if (_property.IsValueMixed)
                {
                    return;
                }

                double currentValue;
                try
                {
                    currentValue = Convert.ToDouble(_property.Value);
                }
                catch (Exception)
                {
                    return;
                }

                this.SetClamped(currentValue >= minLimit && currentValue <= maxLimit);

                if (_attribute.AutoClamp)
                {
                    var clampedValue = Math.Clamp(currentValue, minLimit, maxLimit);

                    const double epsilon = 1e-9;
                    if (Math.Abs(clampedValue - currentValue) > epsilon)
                    {
                        _property.SetValue(ToPropertyTyped(clampedValue));
                        currentValue = clampedValue;
                    }
                }

                SetValueWithoutNotify((float) currentValue);
            }
        }
    }

    public static class SliderExtensions
    {
        private static readonly PropertyInfo Clamped;

        static SliderExtensions()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Clamped = typeof(Slider).GetProperty("clamped", flags);

            if (Clamped == null)
            {
                Debug.LogError("TriInspector failed to access clamped property on Slider. " +
                               $"Please open a bug report. Unity version {Application.unityVersion}");
            }
        }

        public static void SetClamped(this Slider slider, bool value)
        {
            Clamped?.SetValue(slider, value);
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

            internal SliderResolvers(ref HashSet<string> errors, TriPropertyDefinition propertyDefinition,
                SliderAttribute attribute)
                : this(ref errors, propertyDefinition, attribute.MinMemberName, attribute.MaxMemberName,
                    attribute.MinMaxMemberName)
            {
            }

            protected SliderResolvers(ref HashSet<string> errors, TriPropertyDefinition propertyDefinition,
                string minMemberName, string maxMemberName, string minMaxMemberName)
            {
                bool hasMinMaxMember = !string.IsNullOrEmpty(minMaxMemberName);
                if (hasMinMaxMember)
                {
                    minMaxVector2Resolver = ValueResolver.Resolve<Vector2>(propertyDefinition, minMaxMemberName);
                    if (minMaxVector2Resolver.TryGetErrorString(out var vector2Error))
                    {
                        minMaxVector2Resolver = null;
                        minMaxVector2IntResolver =
                            ValueResolver.Resolve<Vector2Int>(propertyDefinition, minMaxMemberName);
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

        public static (double min, double max) GetLimits(TriProperty property, SliderAttribute attribute,
            SliderResolvers resolvers)
        {
            return GetLimits(property, attribute.MinFixed, attribute.MaxFixed, resolvers);
        }

        public static (double min, double max) GetLimits(TriProperty property, float minFixed, float maxFixed,
            SliderResolvers resolvers)
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