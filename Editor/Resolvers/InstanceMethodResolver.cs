using System;
using System.Reflection;

namespace TriInspector.Resolvers
{
    internal class InstanceMethodResolver<TReturnType> : MethodResolver<TReturnType>
    {
        private readonly MethodInfo _methodInfo;

        public static bool TryResolve(TriPropertyDefinition propertyDefinition, string method,
            out MethodResolver<TReturnType> resolver)
        {
            var parentType = propertyDefinition.MemberInfo.DeclaringType;
            if (parentType == null)
            {
                resolver = null;
                return false;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var methodInfo in parentType.GetMethods(flags))
            {
                if (methodInfo.Name == method &&
                    methodInfo.ReturnType == typeof(TReturnType) &&
                    methodInfo.GetParameters() is var parameterInfos &&
                    parameterInfos.Length == 0)
                {
                    resolver = new InstanceMethodResolver<TReturnType>(methodInfo);
                    return true;
                }
            }

            resolver = null;
            return false;
        }

        private InstanceMethodResolver(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
        }

        public override bool TryGetErrorString(out string error)
        {
            error = "";
            return false;
        }

        public override TReturnType InvokeForTarget(TriProperty property, int targetIndex)
        {
            var parentValue = property.Parent.GetValue(targetIndex);

            return (TReturnType) _methodInfo.Invoke(parentValue, Array.Empty<object>());
        }
    }
}