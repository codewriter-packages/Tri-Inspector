using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector.Utilities
{
    internal static class TriUnitySerializationUtilities
    {
        private static readonly HashSet<string> ExcludedNamespaces = new()
        {
            "System",
            "System.IO",
            "System.Net",
            "System.Reflection",
            "System.Threading",
        };

        public static bool IsTypeSupportedBySerializeField(Type type)
        {
            if (type == typeof(object) ||
                type == typeof(IntPtr) ||
                type == typeof(UIntPtr) ||
                type.IsInterface)
            {
                return false;
            }

            if (type.IsPrimitive || type.IsEnum || TriReflectionUtilities.MakeSerializableTypes.Contains(type))
            {
                return true;
            }

            if (ExcludedNamespaces.Contains(type.Namespace))
            {
                return false;
            }

            return true;
        }

        public static bool IsTypeSerializableByUnity(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type.IsArray)
            {
                return true;
            }

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