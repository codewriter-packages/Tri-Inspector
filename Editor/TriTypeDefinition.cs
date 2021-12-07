using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector.Utilities;

namespace TriInspector
{
    internal class TriTypeDefinition
    {
        private static readonly Dictionary<Type, TriTypeDefinition> Cache =
            new Dictionary<Type, TriTypeDefinition>();

        private TriTypeDefinition(Type type)
        {
            var fields = TriReflectionUtilities
                .GetAllInstanceFieldsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => new TriPropertyDefinition(ind + 1, it))
                .ToList();

            var properties = TriReflectionUtilities
                .GetAllInstancePropertiesInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => new TriPropertyDefinition(ind + fields.Count + 1, it))
                .ToList();

            Properties = Enumerable.Empty<TriPropertyDefinition>()
                .Concat(fields)
                .Concat(properties)
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