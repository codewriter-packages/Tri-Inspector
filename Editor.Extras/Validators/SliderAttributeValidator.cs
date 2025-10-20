using System;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Validators;

[assembly: RegisterTriAttributeValidator(typeof(SliderAttributeValidator), ApplyOnArrayElement = true)]

namespace TriInspector.Validators
{
    public class SliderAttributeValidator : TriAttributeValidator<SliderAttribute>
    {
        private SliderAttributeHelpers.SliderResolvers _resolvers;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvers = SliderAttributeHelpers.Initialize(Attribute, propertyDefinition, out var errorResult);
            return errorResult;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            if (Attribute.AutoClamp)
            {
                return TriValidationResult.Valid;
            }

            var (minLimit, maxLimit) = SliderAttributeHelpers.GetLimits(property, Attribute, _resolvers);

            double currentValue;
            try
            {
                currentValue = Convert.ToDouble(property.Value);
            }
            catch (Exception)
            {
                return TriValidationResult.Error("Cannot convert value to a number.");
            }

            if (currentValue < minLimit || currentValue > maxLimit)
            {
                return TriValidationResult.Warning($"Value is out of range [{minLimit:0.##}, {maxLimit:0.##}].")
                    .WithFix(() =>
                    {
                        double clampedValue = Math.Clamp(currentValue, minLimit, maxLimit);
                        property.SetValue(Convert.ChangeType(clampedValue, property.ValueType));
                    }, "Clamp");
            }

            return TriValidationResult.Valid;
        }
    }
}