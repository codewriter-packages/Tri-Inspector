using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector.Utilities;

namespace TriInspector.TypeProcessors
{
    [RegisterTriTypeProcessor(1)]
    public class TriRegisterShownByTriPropertiesTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int propertiesOffset = 10001;

            properties.AddRange(TriReflectionUtilities
                .GetAllInstancePropertiesInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => TriPropertyDefinition.CreateForPropertyInfo(ind + propertiesOffset, it,
                    TriPropertyOrigin.TriProperty)));
        }

        private static bool IsSerialized(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<ShowInInspectorAttribute>(false) != null;
        }
    }
}