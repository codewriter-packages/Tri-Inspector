using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector;
using TriInspector.TypeProcessors;
using TriInspector.Utilities;
using UnityEngine;

[assembly: RegisterTriTypeProcessor(typeof(TriRegisterUnitySerializedFieldsTypeProcessor), 0)]

namespace TriInspector.TypeProcessors
{
    public class TriRegisterUnitySerializedFieldsTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int fieldsOffset = 1;

            var list = TriReflectionUtilities.GetAllInstanceFieldsInDeclarationOrder(type);
            var ind = 0;

            foreach (var fieldInfo in list)
            {
                if (fieldInfo.IsInitOnly)
                {
                    continue;
                }

                if (fieldInfo.GetCustomAttribute<NonSerializedAttribute>() != null ||
                    fieldInfo.GetCustomAttribute<HideInInspector>() != null)
                {
                    continue;
                }

                if (fieldInfo.GetCustomAttribute<SerializeReference>() != null)
                {
                    // if it's a list or array, the base type should be serializable, actually...
                    // but we'll check this in the UnitySerializationRulesAnalyzer and display a warning in the inspector
                    properties.Add(TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo,
                        TriPropertyOrigin.UnitySerializeReference));
                    continue;
                }

                // [Serializable] check moved to UnitySerializationRulesAnalyzer, just skip some dangerous types
                // Unsupported collection types check also moved to analyzer
                if (fieldInfo.GetCustomAttribute<SerializeField>() != null &&
                    TriUnitySerializationUtilities.IsTypeSupportedBySerializeField(fieldInfo.FieldType))
                {
                    properties.Add(TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo,
                        TriPropertyOrigin.UnitySerializeField));
                    continue;
                }

                if (fieldInfo.IsPublic &&
                    TriUnitySerializationUtilities.IsTypeSupportedBySerializeField(fieldInfo.FieldType))
                {
                    properties.Add(TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo,
                        TriPropertyOrigin.UnityPublicField));
                    continue;
                }
            }
        }
    }
}