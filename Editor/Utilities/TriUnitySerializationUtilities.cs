using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TriInspector.Utilities
{
    internal static class TriUnitySerializationUtilities
    {
        public static bool IsSerializableByUnity(FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttribute<NonSerializedAttribute>() != null ||
                fieldInfo.GetCustomAttribute<HideInInspector>() != null)
            {
                return false;
            }

            if (fieldInfo.GetCustomAttribute<SerializeReference>() != null)
            {
                return true;
            }

            if (fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null)
            {
                return IsTypeSerializable(fieldInfo.FieldType);
            }

            return false;
        }

        public static bool IsTypeSerializable(Type type, bool allowCollections = true)
        {
            if (type == typeof(object))
            {
                return false;
            }

            if (type == typeof(string) ||
                type == typeof(bool) ||
                type == typeof(char) ||
                type == typeof(int) ||
                type == typeof(float) ||
                type == typeof(Vector2) ||
                type == typeof(Vector2Int) ||
                type == typeof(Vector3) ||
                type == typeof(Vector3Int) ||
                type == typeof(Vector4) ||
                type == typeof(Color) ||
                type == typeof(Color32) ||
                type == typeof(LayerMask) ||
                type == typeof(Rect) ||
                type == typeof(RectInt) ||
                type == typeof(AnimationCurve) ||
                type == typeof(Bounds) ||
                type == typeof(BoundsInt) ||
                type == typeof(Gradient) ||
                type == typeof(Quaternion))
            {
                return true;
            }

            if (typeof(Object).IsAssignableFrom(type))
            {
                return true;
            }

            if (type.IsEnum)
            {
                return true;
            }

            if (type.IsPrimitive)
            {
                return true;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return allowCollections && IsTypeSerializable(elementType, allowCollections: false);
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(List<>))
                {
                    var elementType = type.GetGenericArguments()[0];
                    return allowCollections && IsTypeSerializable(elementType, allowCollections: false);
                }

                if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    return false;
                }
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return false;
            }

            if (type.GetCustomAttribute<SerializableAttribute>() != null)
            {
                return true;
            }

            // any other cases?

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

            if (type.IsArray && type.GetElementType() is var elementType && elementType != null)
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