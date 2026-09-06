using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace TriInspector.TypeProcessors
{
    [RegisterTriTypeProcessor(0)]
    public class TriRegisterFieldsTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int fieldsOffset = 1;

            using (ListPool<FieldInfo>.Get(out var result))
            {
                TriReflectionUtilities.GetAllInstanceFieldsInDeclarationOrder(result, type);

                var ind = 0;
                foreach (var fieldInfo in result)
                {
                    if (!fieldInfo.IsInitOnly &&
                        !fieldInfo.IsDefined(typeof(NonSerializedAttribute), false) &&
                        !fieldInfo.IsDefined(typeof(HideInInspector), false))
                    {
                        if (fieldInfo.IsDefined(typeof(SerializeReference), false))
                        {
                            // if it's a list or array, the base type should be serializable, actually...
                            // but we'll check this in the UnitySerializationRulesAnalyzer and display a warning in the inspector
                            properties.Add(TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo,
                                TriPropertyOrigin.UnitySerializeReference));
                            continue;
                        }

                        // [Serializable] check moved to UnitySerializationRulesAnalyzer, just skip some dangerous types
                        // Unsupported collection types check also moved to analyzer
                        if (fieldInfo.IsDefined(typeof(SerializeField), false) &&
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

                    if (fieldInfo.IsDefined(typeof(ShowInInspectorAttribute), false))
                    {
                        var property = TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo,
                            TriPropertyOrigin.TriField);

                        properties.Add(property);
                    }
                }
            }
        }
    }
}