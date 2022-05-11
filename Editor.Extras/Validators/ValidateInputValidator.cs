using TriInspector;
using TriInspector.Resolvers;
using TriInspector.Validators;

[assembly: RegisterTriAttributeValidator(typeof(ValidateInputValidator))]

namespace TriInspector.Validators
{
    public class ValidateInputValidator : TriAttributeValidator<ValidateInputAttribute>
    {
        private ValueResolver<TriValidationResult> _resolver;

        public override void Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _resolver = ValueResolver.Resolve<TriValidationResult>(propertyDefinition, Attribute.Method);
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            if (_resolver.TryGetErrorString(out var error))
            {
                return TriValidationResult.Error(error);
            }

            return _resolver.GetValue(property);
        }
    }
}