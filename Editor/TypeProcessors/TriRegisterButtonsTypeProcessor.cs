using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector.Utilities;
using UnityEngine.Pool;

namespace TriInspector.TypeProcessors
{
    [RegisterTriTypeProcessor(3)]
    public class TriRegisterButtonsTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int methodsOffset = 20001;

            using (ListPool<MethodInfo>.Get(out var result))
            {
                TriReflectionUtilities.GetAllInstanceMethodsInDeclarationOrder(result, type);

                var ind = 0;
                foreach (var methodInfo in result)
                {
                    if (!methodInfo.IsDefined(typeof(ButtonAttribute), false))
                    {
                        continue;
                    }

                    var property = TriPropertyDefinition.CreateForMethodInfo(ind++ + methodsOffset, methodInfo);

                    properties.Add(property);
                }
            }
        }
    }
}