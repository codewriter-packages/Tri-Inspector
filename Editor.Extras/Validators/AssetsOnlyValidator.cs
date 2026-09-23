using UnityEditor;
using UnityEngine;

namespace TriInspector.Validators
{
    [RegisterTriAttributeValidator(ApplyOnArrayElement = true)]
    public class AssetsOnlyValidator : TriAttributeValidator<AssetsOnlyAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var targetType = propertyDefinition.IsArrayElement
                ? propertyDefinition.FieldType
                : (propertyDefinition.IsArray ? propertyDefinition.ArrayElementType : propertyDefinition.FieldType);

            if (!typeof(Object).IsAssignableFrom(targetType))
            {
                return "AssetsOnly attribute can be used only on Object fields";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            var obj = property.TryGetSerializedProperty(out var serializedProperty)
                ? serializedProperty.objectReferenceValue
                : (Object) property.Value;

            if (obj == null || AssetDatabase.Contains(obj))
            {
                return TriValidationResult.Valid;
            }

            return TriValidationResult.Error($"{obj} is not an asset.");
        }
    }
}