using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector.Utilities;

namespace TriInspector.TypeProcessors
{
    [RegisterTriTypeProcessor(3)]
    public class TriRegisterButtonsTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int methodsOffset = 20001;

            properties.AddRange(TriReflectionUtilities
                .GetAllInstanceMethodsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => TriPropertyDefinition.CreateForMethodInfo(ind + methodsOffset, it,
                    TriPropertyOrigin.TriButton)));
        }

        private static bool IsSerialized(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<ButtonAttribute>(false) != null;
        }
    }
}