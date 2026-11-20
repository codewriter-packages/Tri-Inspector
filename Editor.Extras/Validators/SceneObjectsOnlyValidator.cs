using UnityEditor;
using UnityEngine;

namespace TriInspector.Validators
{
    [RegisterTriAttributeValidator(ApplyOnArrayElement = true)]
    public class SceneObjectsOnlyValidator : TriAttributeValidator<SceneObjectsOnlyAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var targetType = propertyDefinition.IsArrayElement
                ? propertyDefinition.FieldType
                : (propertyDefinition.IsArray ? propertyDefinition.ArrayElementType : propertyDefinition.FieldType);

            if (!typeof(Object).IsAssignableFrom(targetType))
            {
                return "SceneObjectsOnly attribute can be used only on Object fields";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            var obj = property.TryGetSerializedProperty(out var serializedProperty)
                ? serializedProperty.objectReferenceValue
                : (Object) property.Value;

            if (obj == null || !AssetDatabase.Contains(obj))
            {
                return TriValidationResult.Valid;
            }

            // Children of the prefab being inspected are scene objects once the prefab is instantiated.
            if (serializedProperty != null &&
                serializedProperty.serializedObject.targetObject is var owner && owner != null &&
                AssetDatabase.GetAssetPath(owner) == AssetDatabase.GetAssetPath(obj))
            {
                return TriValidationResult.Valid;
            }

            return TriValidationResult.Error($"{obj} cannot be an asset.");
        }
    }
}