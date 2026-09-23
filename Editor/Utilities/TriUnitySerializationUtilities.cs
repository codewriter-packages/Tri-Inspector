using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace TriInspector.Utilities
{
    internal static class TriUnitySerializationUtilities
    {
        private static readonly Assembly CoreLibAssembly = typeof(List<>).Assembly;
        private static readonly Assembly SystemCoreAssembly = typeof(HashSet<>).Assembly;
        private static readonly Assembly SystemAssembly = typeof(LinkedList<>).Assembly;

        public static bool IsTypeSupportedBySerializeReference(Type type)
        {
            if (type == typeof(object) || type.IsInterface)
            {
                return true;
            }
            
            if (type.IsValueType || type.IsPrimitive || type.IsEnum)
            {
                return false;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return type.GetArrayRank() == 1 &&
                       (IsTypeHasSerializableAttribute(elementType) || elementType.IsInterface);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                return IsTypeHasSerializableAttribute(elementType) || elementType.IsInterface;
            }

            return true;
        }

        public static bool IsTypeSupportedBySerializeField(Type type, bool hasSerializeField,
            bool isCollectionElement = false)
        {
            if (type == typeof(object) ||
                type == typeof(IntPtr) ||
                type == typeof(UIntPtr) ||
                typeof(Delegate).IsAssignableFrom(type) ||
                typeof(ITuple).IsAssignableFrom(type) ||
                type.IsInterface)
            {
                return false;
            }

            if (type.IsPrimitive ||
                TriReflectionUtilities.MakeSerializableTypes.Contains(type) ||
                typeof(Object).IsAssignableFrom(type))
            {
                return true;
            }

            if (type.IsEnum)
            {
                var underlyingType = type.GetEnumUnderlyingType();
                return underlyingType != typeof(long) && underlyingType != typeof(ulong);
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();

                return !isCollectionElement &&
                       type.GetArrayRank() == 1 &&
                       IsTypeSupportedBySerializeField(elementType, hasSerializeField, true);
            }

            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(List<>))
                {
                    var elementType = type.GetGenericArguments()[0];

                    return !isCollectionElement &&
                           IsTypeSupportedBySerializeField(elementType, hasSerializeField, true);
                }

                if (genericType == typeof(Dictionary<,>))
                {
#if UNITY_6000_6
                    return hasSerializeField && !isCollectionElement;
#else
                    return false;
#endif
                }
            }

            if (type.Assembly == CoreLibAssembly ||
                type.Assembly == SystemAssembly ||
                type.Assembly == SystemCoreAssembly)
            {
                return false;
            }

            if (!IsTypeHasSerializableAttribute(type))
            {
                return false;
            }

            return true;
        }

        public static bool IsTypeHasSerializableAttribute(Type type)
        {
            if (type.GetCustomAttribute<SerializableAttribute>() != null)
            {
                return true;
            }

            if (TriReflectionUtilities.MakeSerializableTypes.Contains(type))
            {
                return true;
            }

            return false;
        }

        internal static object PopulateUnityDefaultValueForType(Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            if (typeof(Object).IsAssignableFrom(type))
            {
                return null;
            }

            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                return values.Length > 0 ? values.GetValue(0) : Enum.ToObject(type, 0);
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            if (type.IsArray && type.GetArrayRank() == 1 &&
                type.GetElementType() is var elementType && elementType != null)
            {
                return Array.CreateInstance(elementType, 0);
            }

            if (type.GetConstructor(Type.EmptyTypes) != null)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}