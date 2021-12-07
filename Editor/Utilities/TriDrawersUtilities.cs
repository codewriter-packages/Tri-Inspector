using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector.Elements;

namespace TriInspector.Utilities
{
    internal class TriDrawersUtilities
    {
        private static IDictionary<Type, TriGroupDrawer> _allGroupDrawersCacheBackingField;
        private static IReadOnlyList<RegisterTriDrawerAttribute> _allAttributeDrawerTypesBackingField;
        private static IReadOnlyList<RegisterTriDrawerAttribute> _allValueDrawerTypesBackingField;

        private static IDictionary<Type, TriGroupDrawer> AllGroupDrawersCache
        {
            get
            {
                if (_allGroupDrawersCacheBackingField == null)
                {
                    _allGroupDrawersCacheBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriGroupDrawerAttribute>()
                        let groupAttributeType = IsGroupDrawerType(attr.DrawerType, out var t) ? t : null
                        where groupAttributeType != null
                        select new KeyValuePair<Type, RegisterTriGroupDrawerAttribute>(groupAttributeType, attr)
                    ).ToDictionary(
                        it => it.Key,
                        it => (TriGroupDrawer) Activator.CreateInstance(it.Value.DrawerType));
                }

                return _allGroupDrawersCacheBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriDrawerAttribute> AllValueDrawerTypes
        {
            get
            {
                if (_allValueDrawerTypesBackingField == null)
                {
                    _allValueDrawerTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriDrawerAttribute>()
                        where IsValueDrawerType(attr.DrawerType, out _)
                        select attr
                    ).ToList();
                }

                return _allValueDrawerTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriDrawerAttribute> AllAttributeDrawerTypes
        {
            get
            {
                if (_allAttributeDrawerTypesBackingField == null)
                {
                    _allAttributeDrawerTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriDrawerAttribute>()
                        where IsAttributeDrawerType(attr.DrawerType, out _)
                        select attr
                    ).ToList();
                }

                return _allAttributeDrawerTypesBackingField;
            }
        }

        public static TriPropertyCollectionBaseElement TryCreateGroupElementFor(DeclareGroupBaseAttribute attribute)
        {
            if (!AllGroupDrawersCache.TryGetValue(attribute.GetType(), out var attr))
            {
                return null;
            }

            return attr.CreateElementInternal(attribute);
        }

        private static bool IsGroupDrawerType(Type type, out Type groupAttributeType)
        {
            groupAttributeType = null;

            if (type.IsAbstract)
            {
                return false;
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                return false;
            }

            var drawerType = type.BaseType;

            if (drawerType == null)
            {
                return false;
            }

            if (!drawerType.IsGenericType)
            {
                return false;
            }

            if (drawerType.GetGenericTypeDefinition() != typeof(TriGroupDrawer<>))
            {
                return false;
            }

            groupAttributeType = drawerType.GetGenericArguments()[0];
            return true;
        }

        private static bool IsValueDrawerType(Type type, out Type valueType)
        {
            valueType = null;

            if (type.IsAbstract)
            {
                return false;
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                return false;
            }

            var drawerType = type.BaseType;

            if (drawerType == null)
            {
                return false;
            }

            if (!drawerType.IsGenericType)
            {
                return false;
            }

            if (drawerType.GetGenericTypeDefinition() != typeof(TriValueDrawer<>))
            {
                return false;
            }

            valueType = drawerType.GetGenericArguments()[0];
            return true;
        }

        private static bool IsAttributeDrawerType(Type type, out Type attributeType)
        {
            attributeType = null;

            if (type.IsAbstract)
            {
                return false;
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                return false;
            }

            var drawerType = type.BaseType;

            if (drawerType == null)
            {
                return false;
            }

            if (!drawerType.IsGenericType)
            {
                return false;
            }

            if (drawerType.GetGenericTypeDefinition() != typeof(TriAttributeDrawer<>))
            {
                return false;
            }

            attributeType = drawerType.GetGenericArguments()[0];
            return true;
        }

        public static bool IsValueDrawerFor(Type drawerType, Type valueType)
        {
            if (IsValueDrawerType(drawerType, out var valType))
            {
                return valType == valueType;
            }

            return false;
        }

        public static bool IsAttributeDrawerFor(Type drawerType, Attribute attribute)
        {
            if (IsAttributeDrawerType(drawerType, out var attributeType))
            {
                return attributeType == attribute.GetType();
            }

            return false;
        }
    }
}