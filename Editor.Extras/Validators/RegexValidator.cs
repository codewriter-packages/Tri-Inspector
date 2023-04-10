using System.Text;
using System.Text.RegularExpressions;
using TriInspector;
using TriInspector.Resolvers;
using TriInspector.Validators;

[assembly: RegisterTriAttributeValidator(typeof(RegexValidator))]

namespace TriInspector.Validators
{
    public class RegexValidator : TriAttributeValidator<RegexAttribute>
    {
        private ValueResolver<string> _resolver;
        
        private Regex _regex;
        private readonly StringBuilder _errorStringBuilder = new StringBuilder();

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);
            
            if (propertyDefinition.FieldType != typeof(string))
            {
                return "Scene attribute can only be used on field of type string";
            }
            
            if (Attribute.DynamicExpression && Attribute.Expression.StartsWith("$"))
            {
                _resolver = ValueResolver.ResolveString(propertyDefinition, Attribute.Expression ?? "");
                
                if (_resolver.TryGetErrorString(out var error))
                {
                    return error;
                }
            }
            
            return TriExtensionInitializationResult.Ok;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            var value = property.Value.ToString();
            var expression = Attribute.Expression;

            if (string.IsNullOrEmpty(Attribute.Expression))
            {
                return TriValidationResult.Valid;
            }

            if (Attribute.DynamicExpression && expression.StartsWith("$"))
            {
                if (_resolver.TryGetErrorString(out var error))
                {
                    return TriValidationResult.Error(error);
                }

                expression = _resolver.GetValue(property);
            }

            try
            {
                _regex = new Regex(expression);
            }
            catch
            {
                return TriValidationResult.Error("The expression is invalid");
            }

            if (_regex.IsMatch(value))
            {
                return TriValidationResult.Valid;
            }

            _errorStringBuilder.Append("The value does not match the expression");

            if (Attribute.PreviewExpression)
            {
                _errorStringBuilder.Append($"\nExample: {Attribute.Example}");
            }

            if (!string.IsNullOrEmpty(Attribute.Example))
            {
                _errorStringBuilder.Append($"\nExpression: @\"{expression}\"");
            }

            var errorString = _errorStringBuilder.ToString();

            _errorStringBuilder.Clear();

            return TriValidationResult.Error(errorString);
        }
    }
}