using TriInspector;
using TriInspector.Validators;

[assembly: RegisterTriValueValidator(typeof(TriDictionaryNullKeyEntriesValidator))]

namespace TriInspector.Validators
{
    public class TriDictionaryNullKeyEntriesValidator : TriValueValidator<ITriDictionaryEntry>
    {
        public override TriValidationResult Validate(TriValue<ITriDictionaryEntry> propertyValue)
        {
            var property = propertyValue.Property;

            if (property.IsArrayElement &&
                property.Parent.DictionaryNullKeyEntryIndicesBuffer.Contains(property.IndexInArray))
            {
                return TriValidationResult.Warning(
                    "<b>Null key</b>: " +
                    "A dictionary only stores entries with a valid (non-null) key, " +
                    "so this element is excluded from the runtime dictionary");
            }

            return TriValidationResult.Valid;
        }
    }
}