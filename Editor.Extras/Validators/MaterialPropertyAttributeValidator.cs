using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Validators;
using UnityEngine;

[assembly: RegisterTriAttributeValidator(typeof(MaterialPropertyAttributeValidator), ApplyOnArrayElement = true)]

namespace TriInspector.Validators
{
    public class MaterialPropertyAttributeValidator : TriAttributeValidator<MaterialPropertyAttribute>
    {
        private MaterialPropertyHelper.ResolvedParams _resolvedParams;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvedParams = MaterialPropertyHelper.Initialize(propertyDefinition, Attribute);
            return _resolvedParams.ErrorResult;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            var material = _resolvedParams.MaterialResolver.GetValue(property);

            if (material == null)
            {
                return TriValidationResult.Error("Animator reference is null.");
            }

            if (material.shader == null)
            {
                return TriValidationResult.Error("Material has no shader assigned.");
            }

            var (allParameters, allFilteredParameters) = MaterialPropertyHelper.GetProperties(material, Attribute.PropertyType);

            if (property.ValueType == typeof(string))
            {
                var currentValue = (string)property.Value;

                if (string.IsNullOrEmpty(currentValue))
                {
                    return TriValidationResult.Valid;
                }

                if (!allParameters.Names.Contains(currentValue))
                {
                    string label = MaterialPropertyHelper.GetInvalidPropertyLabel(material, currentValue);
                    return TriValidationResult.Error($"Parameter {label} not found in animator.")
                        .WithFix(() => property.SetValue(null), "Clear value");
                }
                if (!allFilteredParameters.Names.Contains(currentValue))
                {
                    string label = MaterialPropertyHelper.GetInvalidPropertyLabel(material, currentValue);
                    return TriValidationResult.Error($"Parameter {label} is not of the expected type ({Attribute.PropertyType}).")
                        .WithFix(() => property.SetValue(null), "Clear value");
                }
            }

            if (property.ValueType == typeof(int))
            {
                var currentValue = (int)property.Value;
                if (currentValue == 0)
                {
                    return TriValidationResult.Valid;
                }

                if (!allParameters.Hashes.Contains(currentValue))
                {
                    string label = MaterialPropertyHelper.GetInvalidPropertyLabel(material, currentValue);
                    return TriValidationResult.Error($"Parameter {label} not found in animator.")
                        .WithFix(() => property.SetValue(0), "Clear value");
                }
                if (!allFilteredParameters.Hashes.Contains(currentValue))
                {
                    string label = MaterialPropertyHelper.GetInvalidPropertyLabel(material, currentValue);
                    return TriValidationResult.Error($"Parameter {label} is not of the expected type ({Attribute.PropertyType}).")
                        .WithFix(() => property.SetValue(0), "Clear value");
                }
            }

            return TriValidationResult.Valid;
        }
    }
}