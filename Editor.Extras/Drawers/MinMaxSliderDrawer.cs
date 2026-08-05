using System;
using System.Collections.Generic;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

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
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new MinMaxSliderVisualElement(property, Attribute, _resolvers);
        }

        private class MinMaxSliderVisualElement : VisualElement
        {
            private readonly TriProperty _property;
            private readonly MinMaxSliderAttribute _attribute;
            private readonly MinMaxSliderAttributeHelpers.SliderResolvers _resolvers;

            private readonly FloatField _minField;
            private readonly MinMaxSlider _slider;
            private readonly FloatField _maxField;

            public MinMaxSliderVisualElement(TriProperty property, MinMaxSliderAttribute attribute,
                MinMaxSliderAttributeHelpers.SliderResolvers resolvers)
            {
                _property = property;
                _attribute = attribute;
                _resolvers = resolvers;

                var row = new VisualElement();
                row.AddToClassList(Styles.Root);

                _minField = new FloatField();
                _minField.AddToClassList(Styles.Field);
                _minField.RegisterValueChangedCallback(evt => ApplyValue(evt.newValue, CurrentValue().y));

                _slider = new MinMaxSlider();
                _slider.AddToClassList(Styles.Slider);
                _slider.RegisterValueChangedCallback(evt => ApplyValue(evt.newValue.x, evt.newValue.y));

                _maxField = new FloatField();
                _maxField.AddToClassList(Styles.Field);
                _maxField.RegisterValueChangedCallback(evt => ApplyValue(CurrentValue().x, evt.newValue));

                row.Add(_minField);
                row.Add(_slider);
                row.Add(_maxField);

                Add(new TriAlignedLabelVisualElement(property, row));

                RefreshFromProperty();
                this.PeriodicRun(RefreshFromProperty);
            }

            private (double min, double max) GetLimits()
            {
                return MinMaxSliderAttributeHelpers.GetLimits(_property, _attribute, _resolvers);
            }

            private Vector2 CurrentValue()
            {
                switch (_property.Value)
                {
                    case Vector2Int vi: return new Vector2(vi.x, vi.y);
                    case Vector2 v: return v;
                    default: return Vector2.zero;
                }
            }

            private void ApplyValue(float x, float y)
            {
                var (minLimit, maxLimit) = GetLimits();

                // Mirror the IMGUI validation: min can't exceed max and both stay within the limits.
                x = Mathf.Clamp(x, (float) minLimit, Mathf.Min((float) maxLimit, y));
                y = Mathf.Clamp(y, Mathf.Max((float) minLimit, x), (float) maxLimit);

                MinMaxSliderAttributeHelpers.SetValue(_property, x, y);
                RefreshFromProperty();
            }

            private void RefreshFromProperty()
            {
                var (minLimit, maxLimit) = GetLimits();

                _slider.lowLimit = (float) minLimit;
                _slider.highLimit = (float) maxLimit;

                var mixed = _property.IsValueMixed;
                _minField.showMixedValue = mixed;
                _maxField.showMixedValue = mixed;
                _slider.showMixedValue = mixed;

                if (mixed)
                {
                    return;
                }

                var value = CurrentValue();
                var x = value.x;
                var y = value.y;

                if (_attribute.AutoClamp)
                {
                    if (x > y)
                    {
                        (x, y) = (y, x);
                    }

                    x = Mathf.Clamp(x, (float) minLimit, y);
                    y = Mathf.Clamp(y, x, (float) maxLimit);

                    const float epsilon = 1e-5f;
                    if (Mathf.Abs(x - value.x) > epsilon || Mathf.Abs(y - value.y) > epsilon)
                    {
                        MinMaxSliderAttributeHelpers.SetValue(_property, x, y);
                    }
                }

                _minField.SetValueWithoutNotify(x);
                _maxField.SetValueWithoutNotify(y);
                _slider.SetValueWithoutNotify(new Vector2(x, y));
            }

            private static class Styles
            {
                public const string Root = "tri-min-max-slider";
                public const string Field = "tri-min-max-slider__field";
                public const string Slider = "tri-min-max-slider__slider";
            }
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