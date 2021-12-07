using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace TriInspector.Utilities
{
    internal static class TriReflectionUtilities
    {
        private static readonly Dictionary<Type, IReadOnlyList<Attribute>> AttributesCache =
            new Dictionary<Type, IReadOnlyList<Attribute>>();

        private static IReadOnlyList<Assembly> _assemblies;
        private static IReadOnlyList<Type> _allNonAbstractTypesBackingField;

        public static IReadOnlyList<Assembly> Assemblies
        {
            get
            {
                if (_assemblies == null)
                {
                    _assemblies = AppDomain.CurrentDomain.GetAssemblies();
                }

                return _assemblies;
            }
        }

        public static IReadOnlyList<Type> AllNonAbstractTypes
        {
            get
            {
                if (_allNonAbstractTypesBackingField == null)
                {
                    _allNonAbstractTypesBackingField = Assemblies
                        .SelectMany(asm => asm.GetTypes())
                        .Where(type => !type.IsAbstract)
                        .ToList();
                }

                return _allNonAbstractTypesBackingField;
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

        public static IReadOnlyList<T> GetCustomAttributes<T>(this Assembly asm)
        {
            return asm.GetCustomAttributes(typeof(T)).Cast<T>().ToList();
        }

        public static IReadOnlyList<FieldInfo> GetAllInstanceFieldsInDeclarationOrder(Type type)
        {
            var result = new List<FieldInfo>();
            var typeTree = new Stack<Type>();

            while (type != null)
            {
                typeTree.Push(type);
                type = type.BaseType;
            }

            foreach (var t in typeTree)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Instance | BindingFlags.DeclaredOnly;

                var fields = t.GetFields(flags);
                result.AddRange(fields);
            }

            return result;
        }

        public static IReadOnlyList<PropertyInfo> GetAllInstancePropertiesInDeclarationOrder(Type type)
        {
            var result = new List<PropertyInfo>();
            var typeTree = new Stack<Type>();

            while (type != null)
            {
                typeTree.Push(type);
                type = type.BaseType;
            }

            foreach (var t in typeTree)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Instance | BindingFlags.DeclaredOnly;

                var fields = t.GetProperties(flags);
                result.AddRange(fields);
            }

            return result;
        }

        public static bool IsArrayOrList(Type type, out Type elementType)
        {
            if (type.IsArray)
            {
                elementType = type.GetElementType();
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GetGenericArguments().Single();
                return true;
            }

            elementType = null;
            return false;
        }

        public static Type GetUnityEditorTypeByName(string name)
        {
            return GetTypeByName(name, typeof(Editor).Assembly);
        }

        public static Type GetTypeByName(string name, Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Single(it => it.Name == name);
        }
    }
}