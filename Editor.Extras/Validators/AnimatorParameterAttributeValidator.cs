#if TRI_MODULE_ANIMATION

using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Validators;
using UnityEngine;

[assembly: RegisterTriAttributeValidator(typeof(AnimatorParameterAttributeValidator), ApplyOnArrayElement = true)]

namespace TriInspector.Validators
{
    public class AnimatorParameterAttributeValidator : TriAttributeValidator<AnimatorParameterAttribute>
    {
        private AnimatorParameterHelper.ResolvedParams _resolvedParams;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            _resolvedParams = AnimatorParameterHelper.Initialize(propertyDefinition, Attribute);
            return _resolvedParams.ErrorResult;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            var animator = _resolvedParams.AnimatorResolver.GetValue(property);

            if (animator == null)
            {
                return TriValidationResult.Error("Animator reference is null.");
            }

            if (animator.runtimeAnimatorController == null)
            {
                return TriValidationResult.Error("Animator has no controller assigned.");
            }

            var (allParameters, allFilteredParameters) = AnimatorParameterHelper.GetParameters(animator, Attribute.ParameterType);

            if (property.ValueType == typeof(string))
            {
                var currentValue = (string)property.Value;

                if (string.IsNullOrEmpty(currentValue))
                {
                    return TriValidationResult.Valid;
                }

                if (!allParameters.Names.Contains(currentValue))
                {
                    string label = AnimatorParameterHelper.GetInvalidParameterLabel(animator, currentValue);
                    return TriValidationResult.Error($"Parameter {label} not found in animator.")
                        .WithFix(() => property.SetValue(null), "Clear value");
                }
                if (!allFilteredParameters.Names.Contains(currentValue))
                {
                    string label = AnimatorParameterHelper.GetInvalidParameterLabel(animator, currentValue);
                    return TriValidationResult.Error($"Parameter {label} is not of the expected type ({Attribute.ParameterType}).")
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
                    string label = AnimatorParameterHelper.GetInvalidParameterLabel(animator, currentValue);
                    return TriValidationResult.Error($"Parameter {label} not found in animator.")
                        .WithFix(() => property.SetValue(0), "Clear value");
                }
                if (!allFilteredParameters.Hashes.Contains(currentValue))
                {
                    string label = AnimatorParameterHelper.GetInvalidParameterLabel(animator, currentValue);
                    return TriValidationResult.Error($"Parameter {label} is not of the expected type ({Attribute.ParameterType}).")
                        .WithFix(() => property.SetValue(0), "Clear value");
                }
            }

            return TriValidationResult.Valid;
        }
    }
}

#endif