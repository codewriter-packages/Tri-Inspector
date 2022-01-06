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
        private static IReadOnlyList<RegisterTriPropertyHideProcessor> _allHideProcessorTypesBackingField;
        private static IReadOnlyList<RegisterTriPropertyDisableProcessor> _allDisableProcessorTypesBackingField;

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

        public static IReadOnlyList<RegisterTriPropertyHideProcessor> AllHideProcessors
        {
            get
            {
                if (_allHideProcessorTypesBackingField == null)
                {
                    _allHideProcessorTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriPropertyHideProcessor>()
                        where IsHideProcessorType(attr.ProcessorType, out _)
                        select attr
                    ).ToList();
                }

                return _allHideProcessorTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriPropertyDisableProcessor> AllDisableProcessors
        {
            get
            {
                if (_allDisableProcessorTypesBackingField == null)
                {
                    _allDisableProcessorTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriPropertyDisableProcessor>()
                        where IsDisableProcessorType(attr.ProcessorType, out _)
                        select attr
                    ).ToList();
                }

                return _allDisableProcessorTypesBackingField;
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
            return TryGetBaseGenericTargetType(type, typeof(TriGroupDrawer<>), out groupAttributeType);
        }

        private static bool IsValueDrawerType(Type type, out Type valueType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriValueDrawer<>), out valueType);
        }

        private static bool IsAttributeDrawerType(Type type, out Type attributeType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriAttributeDrawer<>), out attributeType);
        }

        private static bool IsHideProcessorType(Type type, out Type attributeType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriPropertyHideProcessor<>), out attributeType);
        }

        private static bool IsDisableProcessorType(Type type, out Type attributeType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriPropertyDisableProcessor<>), out attributeType);
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

        public static bool IsHideProcessorFor(Type processorType, Attribute attribute)
        {
            if (IsHideProcessorType(processorType, out var attributeType))
            {
                return attributeType == attribute.GetType();
            }

            return false;
        }

        public static bool IsDisableProcessorFor(Type processorType, Attribute attribute)
        {
            if (IsDisableProcessorType(processorType, out var attributeType))
            {
                return attributeType == attribute.GetType();
            }

            return false;
        }

        private static bool TryGetBaseGenericTargetType(Type type, Type expectedGenericType, out Type attributeType)
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

            if (drawerType.GetGenericTypeDefinition() != expectedGenericType)
            {
                return false;
            }

            attributeType = drawerType.GetGenericArguments()[0];
            return true;
        }
    }
}