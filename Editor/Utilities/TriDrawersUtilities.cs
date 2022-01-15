using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector.Elements;
using UnityEngine;

namespace TriInspector.Utilities
{
    internal class TriDrawersUtilities
    {
        private static IDictionary<Type, TriGroupDrawer> _allGroupDrawersCacheBackingField;
        private static IReadOnlyList<RegisterTriAttributeDrawerAttribute> _allAttributeDrawerTypesBackingField;
        private static IReadOnlyList<RegisterTriValueDrawerAttribute> _allValueDrawerTypesBackingField;
        private static IReadOnlyList<RegisterTriAttributeValidatorAttribute> _allAttributeValidatorTypesBackingField;
        private static IReadOnlyList<RegisterTriValueValidatorAttribute> _allValueValidatorTypesBackingField;
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

        public static IReadOnlyList<RegisterTriValueDrawerAttribute> AllValueDrawerTypes
        {
            get
            {
                if (_allValueDrawerTypesBackingField == null)
                {
                    _allValueDrawerTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriValueDrawerAttribute>()
                        where IsValueDrawerType(attr.DrawerType, out _)
                        select attr
                    ).ToList();
                }

                return _allValueDrawerTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriAttributeDrawerAttribute> AllAttributeDrawerTypes
        {
            get
            {
                if (_allAttributeDrawerTypesBackingField == null)
                {
                    _allAttributeDrawerTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriAttributeDrawerAttribute>()
                        where IsAttributeDrawerType(attr.DrawerType, out _)
                        select attr
                    ).ToList();
                }

                return _allAttributeDrawerTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriValueValidatorAttribute> AllValueValidatorTypes
        {
            get
            {
                if (_allValueValidatorTypesBackingField == null)
                {
                    _allValueValidatorTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriValueValidatorAttribute>()
                        where IsValueValidatorType(attr.ValidatorType, out _)
                        select attr
                    ).ToList();
                }

                return _allValueValidatorTypesBackingField;
            }
        }

        public static IReadOnlyList<RegisterTriAttributeValidatorAttribute> AllAttributeValidatorTypes
        {
            get
            {
                if (_allAttributeValidatorTypesBackingField == null)
                {
                    _allAttributeValidatorTypesBackingField = (
                        from asm in TriReflectionUtilities.Assemblies
                        from attr in asm.GetCustomAttributes<RegisterTriAttributeValidatorAttribute>()
                        where IsAttributeValidatorType(attr.ValidatorType, out _)
                        select attr
                    ).ToList();
                }

                return _allAttributeValidatorTypesBackingField;
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

        private static bool IsValueValidatorType(Type type, out Type valueType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriValueValidator<>), out valueType);
        }

        private static bool IsAttributeValidatorType(Type type, out Type attributeType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriAttributeValidator<>), out attributeType);
        }

        private static bool IsHideProcessorType(Type type, out Type attributeType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriPropertyHideProcessor<>), out attributeType);
        }

        private static bool IsDisableProcessorType(Type type, out Type attributeType)
        {
            return TryGetBaseGenericTargetType(type, typeof(TriPropertyDisableProcessor<>), out attributeType);
        }

        public static IEnumerable<TriValueDrawer> CreateValueDrawersFor(Type valueType)
        {
            return
                from drawer in AllValueDrawerTypes
                where IsValueDrawerType(drawer.DrawerType, out var vt) &&
                      IsValidTargetType(vt, valueType)
                select CreateInstance<TriValueDrawer>(drawer.DrawerType, valueType, it =>
                {
                    it.ApplyOnArrayElement = drawer.ApplyOnArrayElement;
                    it.Order = drawer.Order;
                });
        }

        public static IEnumerable<TriAttributeDrawer> CreateAttributeDrawersFor(IReadOnlyList<Attribute> attributes)
        {
            return
                from attribute in attributes
                from drawer in AllAttributeDrawerTypes
                where IsAttributeDrawerType(drawer.DrawerType, out var vt) &&
                      IsValidTargetType(vt, attribute.GetType())
                select CreateInstance<TriAttributeDrawer>(drawer.DrawerType, attribute.GetType(), it =>
                {
                    it.ApplyOnArrayElement = drawer.ApplyOnArrayElement;
                    it.Order = drawer.Order;
                    it.RawAttribute = attribute;
                });
        }

        public static IEnumerable<TriValueValidator> CreateValueValidatorsFor(Type valueType)
        {
            return
                from validator in AllValueValidatorTypes
                where IsValueValidatorType(validator.ValidatorType, out var vt) &&
                      IsValidTargetType(vt, valueType)
                select CreateInstance<TriValueValidator>(validator.ValidatorType, valueType,
                    it => { it.ApplyOnArrayElement = validator.ApplyOnArrayElement; });
        }

        public static IEnumerable<TriAttributeValidator> CreateAttributeValidatorsFor(
            IReadOnlyList<Attribute> attributes)
        {
            return
                from attribute in attributes
                from validator in AllAttributeValidatorTypes
                where IsAttributeValidatorType(validator.ValidatorType, out var vt) &&
                      IsValidTargetType(vt, attribute.GetType())
                select CreateInstance<TriAttributeValidator>(validator.ValidatorType, attribute.GetType(), it =>
                {
                    it.ApplyOnArrayElement = validator.ApplyOnArrayElement;
                    it.RawAttribute = attribute;
                });
        }

        public static IEnumerable<TriPropertyHideProcessor> CreateHideProcessorsFor(IReadOnlyList<Attribute> attributes)
        {
            return
                from attribute in attributes
                from processor in AllHideProcessors
                where IsHideProcessorType(processor.ProcessorType, out var vt) &&
                      IsValidTargetType(vt, attribute.GetType())
                select CreateInstance<TriPropertyHideProcessor>(
                    processor.ProcessorType, attributes.GetType(), it =>
                    {
                        //
                        it.RawAttribute = attribute;
                    });
        }

        public static IEnumerable<TriPropertyDisableProcessor> CreateDisableProcessorsFor(
            IReadOnlyList<Attribute> attributes)
        {
            return
                from attribute in attributes
                from processor in AllDisableProcessors
                where IsDisableProcessorType(processor.ProcessorType, out var vt) &&
                      IsValidTargetType(vt, attribute.GetType())
                select CreateInstance<TriPropertyDisableProcessor>(
                    processor.ProcessorType, attributes.GetType(), it =>
                    {
                        //
                        it.RawAttribute = attribute;
                    });
        }

        private static bool IsValidTargetType(Type constraint, Type actual)
        {
            if (constraint == actual)
            {
                return true;
            }

            if (constraint.IsGenericParameter &&
                constraint.GetGenericParameterConstraints().Single().IsAssignableFrom(actual))
            {
                return true;
            }

            return false;
        }

        private static T CreateInstance<T>(Type type, Type argType, Action<T> setup)
        {
            if (type.IsGenericType)
            {
                type = type.MakeGenericType(argType);
            }

            var instance = (T) Activator.CreateInstance(type);
            setup(instance);
            return instance;
        }

        private static bool TryGetBaseGenericTargetType(Type type, Type expectedGenericType, out Type attributeType)
        {
            attributeType = null;

            if (type.IsAbstract)
            {
                Debug.LogError($"{type.Name} must be non abstract");
                return false;
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                Debug.LogError($"{type.Name} must have a parameterless constructor");
                return false;
            }

            Type genericArg = null;
            if (type.IsGenericType)
            {
                genericArg = type.GetGenericArguments().SingleOrDefault();

                if (genericArg == null ||
                    genericArg.GenericParameterAttributes != GenericParameterAttributes.None)
                {
                    Debug.LogError(
                        $"{type.Name} must contains only one generic arg with simple constant e.g. <where T : bool>");
                    return false;
                }

                var argConstraints = genericArg.GetGenericParameterConstraints().SingleOrDefault();
                if (argConstraints == null)
                {
                    Debug.LogError(
                        $"{type.Name} must contains only one generic arg with simple constant e.g. <where T : bool>");
                    return false;
                }
            }

            var drawerType = type.BaseType;

            while (drawerType != null)
            {
                if (drawerType.IsGenericType &&
                    drawerType.GetGenericTypeDefinition() == expectedGenericType)
                {
                    attributeType = drawerType.GetGenericArguments()[0];

                    if (genericArg != null && !attributeType.IsGenericParameter)
                    {
                        Debug.LogError(
                            $"{type.Name} must pass generic arg {genericArg} to {expectedGenericType} base type");
                        return false;
                    }

                    return true;
                }

                drawerType = drawerType.BaseType;
            }

            Debug.LogError($"{type.Name} must implement {expectedGenericType}");
            return false;
        }
    }
}