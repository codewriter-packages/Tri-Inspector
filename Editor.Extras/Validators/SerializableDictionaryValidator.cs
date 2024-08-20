using TriInspector;
using TriInspector.Types;
using TriInspector.Validators;

[assembly: RegisterTriValueValidator(typeof(SerializableDictionaryValidator))]

namespace TriInspector.Validators
{
    public class SerializableDictionaryValidator : TriValueValidator<SerializableDictionary<object, object>>
    {
        public override TriValidationResult Validate(TriValue<SerializableDictionary<object, object>> propertyValue)
        {
            var keyValuePairsProperty = propertyValue.Property.ChildrenProperties[0];

            for (var i = 0; i < keyValuePairsProperty.ArrayElementProperties.Count; i++)
            {
                var keyValuePairLeft = keyValuePairsProperty.ArrayElementProperties[i];
                
                for (var j = i + 1; j < keyValuePairsProperty.ArrayElementProperties.Count; j++)
                {
                    var keyValuePairRight = keyValuePairsProperty.ArrayElementProperties[j];

                    if (keyValuePairLeft.Value != null && keyValuePairRight.Value != null &&
                        keyValuePairLeft.Value.Equals(keyValuePairRight.Value))
                    {
                        return TriValidationResult.Error("Duplicate key detected");
                    }
                }
            }

            return TriValidationResult.Valid;
        }
    }
}