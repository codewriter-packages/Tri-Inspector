using System;
using System.Reflection;
using TriInspector;
using TriInspector.Resolvers;
using TriInspector.Validators;

[assembly: RegisterTriAttributeValidator(typeof(ValidateInputValidator))]

namespace TriInspector.Validators
{
    public class ValidateInputValidator : TriAttributeValidator<ValidateInputAttribute>
    {
        private MethodResolver<TriValidationResult> _resolver;

        public override void Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _resolver = MethodResolver.Resolve<TriValidationResult>(propertyDefinition, Attribute.Method);
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            if (_resolver.TryGetErrorString(out var error))
            {
                return TriValidationResult.Error(error);
            }

            for (var targetIndex = 0; targetIndex < property.PropertyTree.TargetObjects.Length; targetIndex++)
            {
                TriValidationResult result;
                try
                {
                    result = _resolver.InvokeForTarget(property, targetIndex);
                }
                catch (Exception e)
                {
                    if (e is TargetInvocationException targetInvocationException)
                    {
                        e = targetInvocationException.InnerException;
                    }

                    result = TriValidationResult.Error($"Exception was thrown: {e}");
                }

                if (!result.IsValid)
                {
                    return result;
                }
            }

            return TriValidationResult.Valid;
        }
    }
}