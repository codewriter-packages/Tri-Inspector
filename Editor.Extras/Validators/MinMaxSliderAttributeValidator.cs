using System;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Validators;
using UnityEngine;

[assembly: RegisterTriAttributeValidator(typeof(MinMaxSliderAttributeValidator), ApplyOnArrayElement = true)]

namespace TriInspector.Validators
{
    public class MinMaxSliderAttributeValidator : TriAttributeValidator<MinMaxSliderAttribute>
    {
        private MinMaxSliderAttributeHelpers.SliderResolvers _resolvers;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvers = MinMaxSliderAttributeHelpers.Initialize(Attribute, propertyDefinition, out var errorResult);
            return errorResult;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            if (Attribute.AutoClamp)
            {
                return TriValidationResult.Valid;
            }

            var (minLimit, maxLimit) = MinMaxSliderAttributeHelpers.GetLimits(property, Attribute, _resolvers);

            float xValue, yValue;
            if (property.ValueType == typeof(Vector2Int))
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

            bool isInvalid = xValue < minLimit || yValue > maxLimit || xValue > yValue;

            if (isInvalid)
            {
                return TriValidationResult.Warning($"Value is out of range [{minLimit:0.##}, {maxLimit:0.##}].")
                    .WithFix(() =>
                    {
                        float clampedX = xValue;
                        float clampedY = yValue;

                        if (clampedX > clampedY) (clampedX, clampedY) = (clampedY, clampedX);

                        clampedX = Mathf.Clamp(clampedX, (float) minLimit, clampedY);
                        clampedY = Mathf.Clamp(clampedY, clampedX, (float) maxLimit);

                        MinMaxSliderAttributeHelpers.SetValue(property, clampedX, clampedY);
                    }, "Clamp");
            }

            return TriValidationResult.Valid;
        }
    }
}