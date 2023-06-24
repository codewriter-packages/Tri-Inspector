using JetBrains.Annotations;
using TriInspector.Validators;
using TriInspector;
using TriInspector.Resolvers;

[assembly: RegisterTriAttributeValidator(typeof(RequiredValidator), ApplyOnArrayElement = true)]

namespace TriInspector.Validators
{
    public class RequiredValidator : TriAttributeValidator<RequiredAttribute>
    {
        [CanBeNull] private ActionResolver _fixActionResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (Attribute.FixAction != null)
            {
                _fixActionResolver = ActionResolver.Resolve(propertyDefinition, Attribute.FixAction);
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            if (property.FieldType == typeof(string))
            {
                var isNull = string.IsNullOrEmpty((string) property.Value);
                if (isNull)
                {
                    var message = Attribute.Message ?? $"{GetName(property)} is required";
                    return MakeError(message, property);
                }
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(property.FieldType))
            {
                var isNull = null == (UnityEngine.Object) property.Value;
                if (isNull)
                {
                    var message = Attribute.Message ?? $"{GetName(property)} is required";
                    return MakeError(message, property);
                }
            }
            else
            {
                return TriValidationResult.Error("RequiredAttribute only valid on Object and String");
            }

            return TriValidationResult.Valid;
        }

        private TriValidationResult MakeError(string error, TriProperty property)
        {
            var result = TriValidationResult.Error(error);

            if (_fixActionResolver != null)
            {
                result = AddFix(result, property);
            }

            return result;
        }

        private TriValidationResult AddFix(TriValidationResult result, TriProperty property)
        {
            return result.WithFix(() => _fixActionResolver?.InvokeForAllTargets(property), Attribute.FixActionName);
        }

        private static string GetName(TriProperty property)
        {
            var name = property.DisplayName;
            if (string.IsNullOrEmpty(name))
            {
                name = property.RawName;
            }

            return name;
        }
    }
}