using TriInspector;
using TriInspector.Validators;

[assembly:RegisterTriValueValidator(typeof(TriDictionaryDuplicatedEntriesValidator))]

namespace TriInspector.Validators
{
    public class TriDictionaryDuplicatedEntriesValidator : TriValueValidator<ITriDictionaryEntry>
    {
        public override TriValidationResult Validate(TriValue<ITriDictionaryEntry> propertyValue)
        {
            var property = propertyValue.Property;

            if (property.IsArrayElement &&
                property.Parent.DictionaryDuplicateEntryIndicesBuffer.Contains(property.IndexInArray))
            {
                return TriValidationResult.Warning(
                    "<b>Duplicate key</b>: " +
                    "An element with the same key already exist, " +
                    "so this element is excluded from the runtime dictionary");
            }
            
            return TriValidationResult.Valid;
        }
    }
}