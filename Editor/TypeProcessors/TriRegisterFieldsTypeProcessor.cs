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
                            if (TriUnitySerializationUtilities.IsTypeSupportedBySerializeReference(fieldInfo.FieldType))
                            {
                                properties.Add(TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo));
                            }

                            continue;
                        }

                        if (fieldInfo.IsDefined(typeof(SerializeField), false))
                        {
                            if (TriUnitySerializationUtilities.IsTypeSupportedBySerializeField(fieldInfo.FieldType, true))
                            {
                                properties.Add(TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo));
                            }

                            continue;
                        }

                        if (fieldInfo.IsPublic)
                        {
                            if (TriUnitySerializationUtilities.IsTypeSupportedBySerializeField(fieldInfo.FieldType, false))
                            {
                                properties.Add(TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo));
                                continue;
                            }

                            // fall through: an unsupported public type may still be shown via [ShowInInspector]
                        }
                    }

                    if (fieldInfo.IsDefined(typeof(ShowInInspectorAttribute), false))
                    {
                        var property = TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo);

                        properties.Add(property);
                    }
                }
            }
        }
    }
}