using TriInspector;
using TriInspector.Validators;
using UnityEditor;

[assembly: RegisterTriValueValidator(typeof(MissingReferenceValidator<>))]

namespace TriInspector.Validators
{
    public class MissingReferenceValidator<T> : TriValueValidator<T>
        where T : UnityEngine.Object
    {
        public override TriValidationResult Validate(TriValue<T> propertyValue)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out var serializedProperty) &&
                serializedProperty.propertyType == SerializedPropertyType.ObjectReference &&
                serializedProperty.objectReferenceValue == null &&
                serializedProperty.objectReferenceInstanceIDValue != 0)
            {
                return TriValidationResult.Warning($"{propertyValue.Property.DisplayName} is missing");
            }

            return TriValidationResult.Valid;
        }
    }
}