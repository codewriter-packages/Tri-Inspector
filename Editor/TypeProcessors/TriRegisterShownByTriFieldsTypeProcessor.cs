using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector.Utilities;

namespace TriInspector.TypeProcessors
{
    [RegisterTriTypeProcessor(1)]
    public class TriRegisterShownByTriFieldsTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int fieldsOffset = 5001;

            properties.AddRange(TriReflectionUtilities
                .GetAllInstanceFieldsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => TriPropertyDefinition.CreateForFieldInfo(ind + fieldsOffset, it,
                    TriPropertyOrigin.TriField)));
        }

        private static bool IsSerialized(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttribute<ShowInInspectorAttribute>(false) != null;
        }
    }
}