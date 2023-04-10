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
        private string _expression;
        private readonly StringBuilder _errorStringBuilder = new StringBuilder();

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);
            
            if (propertyDefinition.FieldType != typeof(string))
            {
                return "Scene attribute can only be used on field of type string";
            }
            
            if (string.IsNullOrEmpty(Attribute.Expression))
            {
                return TriExtensionInitializationResult.Skip;
            }
            
            if (Attribute.DynamicExpression)
            {
                _expression = Attribute.Expression.StartsWith("$")
                    ? Attribute.Expression.Substring(1)
                    : Attribute.Expression;
                
                _resolver = ValueResolver.Resolve<string>(propertyDefinition, _expression);
                
                if (_resolver.TryGetErrorString(out var error))
                {
                    return error;
                }
            }
            else
            {
                _expression = Attribute.Expression;
            }
            
            return TriExtensionInitializationResult.Ok;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            if (string.IsNullOrEmpty(_expression))
            {
                return TriValidationResult.Warning("Expression is null or empty");
            }

            var value = property.Value.ToString();
            var expression = _expression;

            if (Attribute.DynamicExpression)
            {
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