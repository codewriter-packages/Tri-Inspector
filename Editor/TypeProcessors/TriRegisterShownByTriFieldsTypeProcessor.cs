using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector.Utilities;
using UnityEngine.Pool;

namespace TriInspector.TypeProcessors
{
    [RegisterTriTypeProcessor(1)]
    public class TriRegisterShownByTriFieldsTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int fieldsOffset = 5001;

            using (ListPool<FieldInfo>.Get(out var result))
            {
                TriReflectionUtilities.GetAllInstanceFieldsInDeclarationOrder(result, type);

                var ind = 0;
                foreach (var fieldInfo in result)
                {
                    if (!fieldInfo.IsDefined(typeof(ShowInInspectorAttribute), false))
                    {
                        continue;
                    }

                    var property = TriPropertyDefinition.CreateForFieldInfo(ind++ + fieldsOffset, fieldInfo,
                        TriPropertyOrigin.TriField);

                    properties.Add(property);
                }
            }
        }
    }
}