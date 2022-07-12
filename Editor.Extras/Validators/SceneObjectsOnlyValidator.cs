﻿using TriInspector;
using TriInspector.Validators;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeValidator(typeof(SceneObjectsOnlyValidator))]

namespace TriInspector.Validators
{
    public class SceneObjectsOnlyValidator : TriAttributeValidator<SceneObjectsOnlyAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!typeof(Object).IsAssignableFrom(propertyDefinition.FieldType))
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

            if (obj == null || !AssetDatabase.Contains(obj))
            {
                return TriValidationResult.Valid;
            }

            return TriValidationResult.Error($"{obj} cannot be an asset.");
        }
    }
}