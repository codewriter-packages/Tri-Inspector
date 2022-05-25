using Sirenix.OdinInspector.Editor.Validation;

namespace TriInspector.Editor.Integrations.Odin
{
    internal static class TriOdinExtensions
    {
        public static void CopyValidationResultsTo(this TriPropertyTree tree, ValidationResult result)
        {
            tree.EnumerateValidationResults(result.AddIfError);
            tree.EnumerateValidationResults(result.AddIfWarning);
        }

        private static void AddIfError(this ValidationResult result,
            TriProperty property, TriValidationResult triResult)
        {
            if (triResult.MessageType != TriMessageType.Error)
            {
                return;
            }

            result.AddError(triResult.Message);
        }

        private static void AddIfWarning(this ValidationResult result,
            TriProperty property, TriValidationResult triResult)
        {
            if (triResult.MessageType != TriMessageType.Warning)
            {
                return;
            }

            result.AddWarning(triResult.Message);
        }
    }
}