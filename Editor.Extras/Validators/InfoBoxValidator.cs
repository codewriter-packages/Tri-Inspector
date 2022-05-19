using TriInspector;
using TriInspector.Resolvers;
using TriInspector.Validators;

[assembly: RegisterTriAttributeValidator(typeof(InfoBoxValidator))]

namespace TriInspector.Validators
{
    public class InfoBoxValidator : TriAttributeValidator<InfoBoxAttribute>
    {
        private ValueResolver<string> _resolver;
        private ValueResolver<bool> _visibleIfResolver;

        public override void Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _resolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Text);
            _visibleIfResolver = Attribute.VisibleIf != null
                ? ValueResolver.Resolve<bool>(propertyDefinition, Attribute.VisibleIf)
                : null;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            if (_visibleIfResolver != null && !_visibleIfResolver.GetValue(property))
            {
                return TriValidationResult.Valid;
            }

            var message = _resolver.GetValue(property, "");
            return new TriValidationResult(false, message, Attribute.MessageType);
        }
    }
}