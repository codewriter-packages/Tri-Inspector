using System;
using System.Reflection;
using TriInspector.Utilities;
using UnityEngine;

namespace TriInspector.Resolvers
{
    public class StaticMethodValueResolver<T> : ValueResolver<T>
    {
        private readonly MethodInfo _methodInfo;

        public static bool TryResolve(TriPropertyDefinition propertyDefinition, string expression,
            out ValueResolver<T> resolver)
        {
            var type = propertyDefinition.OwnerType;
            var methodName = expression;
            
            var separatorIndex = expression.LastIndexOf('.');
            if (separatorIndex >= 0)
            {
                var className = expression.Substring(0, separatorIndex);
                methodName = expression.Substring(separatorIndex + 1);

                if (!TriReflectionUtilities.TryFindTypeByFullName(className, out type))
                {
                    resolver = null;
                    return false;
                }
            }

            if (type == null)
            {
                resolver = null;
                return false;
            }

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var methodInfo in type.GetMethods(flags))
            {
                if (methodInfo.Name == methodName &&
                    typeof(T).IsAssignableFrom(methodInfo.ReturnType) &&
                    methodInfo.GetParameters() is var parametersInfo &&
                    parametersInfo.Length == 0)
                {
                    resolver = new StaticMethodValueResolver<T>(methodInfo);
                    return true;
                }
            }

            resolver = null;
            return false;
        }

        public StaticMethodValueResolver(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
        }

        public override bool TryGetErrorString(out string error)
        {
            error = "";
            return false;
        }

        public override T GetValue(TriProperty property, T defaultValue = default)
        {
            try
            {
                return (T) _methodInfo.Invoke(null, null);
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException targetInvocationException)
                {
                    e = targetInvocationException.InnerException;
                }

                Debug.LogException(e);
                return defaultValue;
            }
        }
    }
}