using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector.Utilities;
using UnityEngine.Pool;

namespace TriInspector.TypeProcessors
{
    [RegisterTriTypeProcessor(1)]
    public class TriRegisterPropertiesTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int propertiesOffset = 10001;

            using (ListPool<PropertyInfo>.Get(out var result))
            {
                TriReflectionUtilities.GetAllInstancePropertiesInDeclarationOrder(result, type);

                var ind = 0;
                foreach (var propertyInfo in result)
                {
                    if (!propertyInfo.IsDefined(typeof(ShowInInspectorAttribute), false))
                    {
                        continue;
                    }

                    var property = TriPropertyDefinition.CreateForPropertyInfo(ind++ + propertiesOffset, propertyInfo);

                    properties.Add(property);
                }
            }
        }
    }
}