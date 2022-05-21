using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;

namespace TriInspector.Editor.Integrations.Odin
{
    internal static class TriOdinExtensions
    {
        public static void CopyValidationResultsTo(this TriPropertyTree tree, ValidationResult result)
        {
            tree.EnumerateValidationResults((property, triResult) => result.Add(triResult));
        }

        private static void Add(this ValidationResult result, TriValidationResult src)
        {
            var severity = src.GetOdinValidatorSeverity();
            result.Add(severity, src.Message);
        }

        private static ValidatorSeverity GetOdinValidatorSeverity(this TriValidationResult result)
        {
            switch (result.MessageType)
            {
                case TriMessageType.Error:
                    return ValidatorSeverity.Error;

                case TriMessageType.Warning:
                    return ValidatorSeverity.Warning;
            }

            return ValidatorSeverity.Ignore;
        }
    }
}