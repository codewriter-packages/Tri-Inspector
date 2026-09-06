using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    internal static class TriReflectionUtilities
    {
        private static readonly Dictionary<Type, IReadOnlyList<Attribute>> AttributesCache =
            new Dictionary<Type, IReadOnlyList<Attribute>>();

        private static ISet<Type> _makeSerializableTypes;

        private static IReadOnlyList<Assembly> GetAssemblies()
        {
#if UNITY_6000_6_OR_NEWER
            return UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies();
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        public static ISet<Type> MakeSerializableTypes
        {
            get
            {
                if (_makeSerializableTypes == null)
                {
                    var set = new HashSet<Type>();
                    _makeSerializableTypes = set;

                    set.Add(typeof(string));
                    set.Add(typeof(Vector2));
                    set.Add(typeof(Vector2Int));
                    set.Add(typeof(Vector3));
                    set.Add(typeof(Vector3Int));
                    set.Add(typeof(Vector4));
                    set.Add(typeof(Color));
                    set.Add(typeof(Color32));
                    set.Add(typeof(LayerMask));
                    set.Add(typeof(Rect));
                    set.Add(typeof(RectInt));
                    set.Add(typeof(AnimationCurve));
                    set.Add(typeof(Bounds));
                    set.Add(typeof(BoundsInt));
                    set.Add(typeof(Gradient));
                    set.Add(typeof(Quaternion));
                    set.Add(typeof(PropertyName));

#if UNITY_6000_6
                    var getSerializableType = typeof(MakeSerializableAttribute).GetMethod("GetSerializableType",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (getSerializableType != null)
                    {
                        var found = GetAssemblies()
                            .SelectMany(asm => asm.GetCustomAttributes(typeof(MakeSerializableAttribute)))
                            .Select(attr => (Type) getSerializableType.Invoke(attr, null));

                        foreach (var type in found)
                        {
                            set.Add(type);
                        }
                    }
                    else
                    {
                        Debug.LogError(
                            "TriInspector failed to access GetSerializableType method on MakeSerializableAttribute. " +
                            $"Please open a bug report. Unity version {Application.unityVersion}");
                    }
#endif
                }

                return _makeSerializableTypes;
            }
        }

        public static IReadOnlyList<Attribute> GetAttributesCached(Type type)
        {
            if (AttributesCache.TryGetValue(type, out var attributes))
            {
                return attributes;
            }

            return AttributesCache[type] = type.GetCustomAttributes().ToList();
        }

        public static Attribute[] GetCustomNonSerializationAttributes(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return null;
            }

            if (!HasAnyNonSerializationAttributes(memberInfo))
            {
                return null;
            }

            return Attribute.GetCustomAttributes(memberInfo);
        }

        private static bool HasAnyNonSerializationAttributes(MemberInfo memberInfo)
        {
            foreach (var customAttributeData in memberInfo.GetCustomAttributesData())
            {
                if (!SerializationOnlyAttributeTypes.Contains(customAttributeData.AttributeType))
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly HashSet<Type> SerializationOnlyAttributeTypes = new HashSet<Type>
        {
            typeof(SerializeField),
            typeof(SerializeReference),
            typeof(ShowInInspectorAttribute),
        };

        public static void GetAllInstanceFieldsInDeclarationOrder(List<FieldInfo> result, Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Instance | BindingFlags.DeclaredOnly;

            GetAllMembersInDeclarationOrder(result, type, static it => it.GetFields(flags));
        }

        public static void GetAllInstancePropertiesInDeclarationOrder(List<PropertyInfo> result, Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Instance | BindingFlags.DeclaredOnly;

            GetAllMembersInDeclarationOrder(result, type, static it => it.GetProperties(flags));
        }

        public static void GetAllInstanceMethodsInDeclarationOrder(List<MethodInfo> result, Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Instance | BindingFlags.DeclaredOnly;

            GetAllMembersInDeclarationOrder(result, type, static it => it.GetMethods(flags));
        }

        public static bool IsArrayOrListOrDictionary(Type type, out Type elementType, out bool isDictionary)
        {
            if (type.IsArray && type.GetArrayRank() == 1)
            {
                elementType = type.GetElementType();
                isDictionary = false;
                return true;
            }

            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(List<>))
                {
                    elementType = type.GetGenericArguments().Single();
                    isDictionary = false;
                    return true;
                }

                if (genericType == typeof(Dictionary<,>))
                {
                    elementType = Array
                        .Find(type.GetInterfaces(),
                            it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(ICollection<>))
                        .GetGenericArguments().Single();
                    isDictionary = true;
                    return true;
                }
            }

            elementType = null;
            isDictionary = false;
            return false;
        }

        public static bool TryFindTypeByFullName(string name, out Type type)
        {
            type = Type.GetType(name);
            if (type != null)
            {
                return true;
            }

            foreach (var assembly in GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void GetAllMembersInDeclarationOrder<T>(List<T> result, Type type, Func<Type, T[]> select)
            where T : MemberInfo
        {
            var typeTree = new Stack<Type>();

            while (type != null)
            {
                typeTree.Push(type);
                type = type.BaseType;
            }

            foreach (var t in typeTree)
            {
                var items = select(t);
                result.AddRange(items);
            }
        }
    }
}