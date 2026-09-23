using System;
using System.Reflection;
using TriInspector.Utilities;
using UnityEngine;

namespace TriInspector.Resolvers
{
    public class StaticPropertyValueResolver<T> : ValueResolver<T>
    {
        private readonly PropertyInfo _propertyInfo;

        public static bool TryResolve(TriPropertyDefinition propertyDefinition, string expression,
            out ValueResolver<T> resolver)
        {
            var type = propertyDefinition.OwnerType;
            var propertyName = expression;
            
            var separatorIndex = expression.LastIndexOf('.');
            if (separatorIndex >= 0)
            {
                var className = expression.Substring(0, separatorIndex);
                propertyName = expression.Substring(separatorIndex + 1);

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

            foreach (var propertyInfo in type.GetProperties(flags))
            {
                if (propertyInfo.Name == propertyName &&
                    typeof(T).IsAssignableFrom(propertyInfo.PropertyType) &&
                    propertyInfo.CanRead)
                {
                    resolver = new StaticPropertyValueResolver<T>(propertyInfo);
                    return true;
                }
            }

            resolver = null;
            return false;
        }

        public StaticPropertyValueResolver(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
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
                return (T) _propertyInfo.GetValue(null);
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