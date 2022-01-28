using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector.Utilities;

namespace TriInspector
{
    public class TriTypeDefinition
    {
        private static readonly Dictionary<Type, TriTypeDefinition> Cache =
            new Dictionary<Type, TriTypeDefinition>();

        private TriTypeDefinition(Type type)
        {
            var fieldsOffset = 1;
            var fields = TriReflectionUtilities
                .GetAllInstanceFieldsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => new TriPropertyDefinition(ind + fieldsOffset, it))
                .ToList();

            var propertiesOffset = fieldsOffset + fields.Count;
            var properties = TriReflectionUtilities
                .GetAllInstancePropertiesInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => new TriPropertyDefinition(ind + propertiesOffset, it))
                .ToList();

            var methodsOffset = propertiesOffset + properties.Count;
            var methods = TriReflectionUtilities
                .GetAllInstanceMethodsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => new TriPropertyDefinition(ind + methodsOffset, it))
                .ToList();

            Properties = Enumerable.Empty<TriPropertyDefinition>()
                .Concat(fields)
                .Concat(properties)
                .Concat(methods)
                .OrderBy(it => it.Order)
                .ToList();
        }

        public IReadOnlyList<TriPropertyDefinition> Properties { get; }

        private static bool IsSerialized(FieldInfo fieldInfo)
        {
            return TriUnitySerializationUtilities.IsSerializableByUnity(fieldInfo);
        }

        private static bool IsSerialized(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<ShowInInspector>() != null;
        }

        private static bool IsSerialized(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<ButtonAttribute>() != null;
        }

        public static TriTypeDefinition GetCached(Type type)
        {
            if (Cache.TryGetValue(type, out var definition))
            {
                return definition;
            }

            return Cache[type] = new TriTypeDefinition(type);
        }
    }
}