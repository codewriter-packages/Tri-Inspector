using System;
using System.Reflection;
using TriInspector;
using TriInspector.Validators;

[assembly: RegisterTriAttributeValidator(typeof(ValidateInputValidator), ApplyOnArrayElement = true)]

namespace TriInspector.Validators
{
    public class ValidateInputValidator : TriAttributeValidator<ValidateInputAttribute>
    {
        public override TriValidationResult Validate(TriProperty property)
        {
            var methodName = Attribute.Method;

            for (var targetIndex = 0; targetIndex < property.PropertyTree.TargetObjects.Length; targetIndex++)
            {
                var parentValue = property.Parent.GetValue(targetIndex);
                var parentType = parentValue.GetType();

                if (!TryFindValidationMethod(parentType, methodName, out var methodInfo))
                {
                    return TriValidationResult.Error($"Method '{methodName}' not exists or has wrong signature");
                }

                TriValidationResult result;
                try
                {
                    result = (TriValidationResult) methodInfo.Invoke(parentValue, Array.Empty<object>());
                }
                catch (Exception e)
                {
                    if (e is TargetInvocationException targetInvocationException)
                    {
                        e = targetInvocationException.InnerException;
                    }

                    result = TriValidationResult.Error($"Exception was thrown: {e}'");
                }

                if (!result.IsValid)
                {
                    return result;
                }
            }

            return TriValidationResult.Valid;
        }

        private static bool TryFindValidationMethod(Type type, string method, out MethodInfo result)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var methodInfo in type.GetMethods(flags))
            {
                if (methodInfo.Name == method &&
                    methodInfo.ReturnType == typeof(TriValidationResult) &&
                    methodInfo.GetParameters() is var parameterInfos &&
                    parameterInfos.Length == 0)
                {
                    result = methodInfo;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}