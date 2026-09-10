using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Utilities
{
    internal class TriDrawersUtilities
    {
        private static readonly GenericTypeMatcher GroupDrawerMatcher = typeof(TriGroupDrawer<>);
        private static readonly GenericTypeMatcher ValueDrawerMatcher = typeof(TriValueDrawer<>);
        private static readonly GenericTypeMatcher AttributeDrawerMatcher = typeof(TriAttributeDrawer<>);
        private static readonly GenericTypeMatcher ValueValidatorMatcher = typeof(TriValueValidator<>);
        private static readonly GenericTypeMatcher AttributeValidatorMatcher = typeof(TriAttributeValidator<>);
        private static readonly GenericTypeMatcher HideProcessorMatcher = typeof(TriPropertyHideProcessor<>);
        private static readonly GenericTypeMatcher DisableProcessorMatcher = typeof(TriPropertyDisableProcessor<>);

        private static IDictionary<Type, TriGroupDrawer> _allGroupDrawersCacheBackingField;
        private static TriArray<Info<RegisterTriAttributeDrawerAttribute>>? _allAttributeDrawerTypesBackingField;
        private static TriArray<Info<RegisterTriValueDrawerAttribute>>? _allValueDrawerTypesBackingField;
        private static TriArray<Info<RegisterTriAttributeValidatorAttribute>>? _allAttributeValidatorTypesBackingField;
        private static TriArray<Info<RegisterTriValueValidatorAttribute>>? _allValueValidatorTypesBackingField;
        private static TriArray<Info<RegisterTriPropertyHideProcessor>>? _allHideProcessorTypesBackingField;
        private static TriArray<Info<RegisterTriPropertyDisableProcessor>>? _allDisableProcessorTypesBackingField;

        private static TriArray<TriTypeProcessor>? _allTypeProcessorBackingField;

        private static IDictionary<Type, TriGroupDrawer> AllGroupDrawersCache
        {
            get
            {
                if (_allGroupDrawersCacheBackingField == null)
                {
                    _allGroupDrawersCacheBackingField = (
                        from drawerType in TypeCache.GetTypesDerivedFrom(typeof(TriGroupDrawer))
                        let attr = drawerType.GetCustomAttribute<RegisterTriGroupDrawerAttribute>()
                        where attr != null
                        let groupAttributeType = GroupDrawerMatcher.MatchOut(drawerType, out var t) ? t : null
                        where groupAttributeType != null
                        select new KeyValuePair<Type, Type>(groupAttributeType, drawerType)
                    ).ToDictionary(
                        it => it.Key,
                        it => (TriGroupDrawer) Activator.CreateInstance(it.Value));
                }

                return _allGroupDrawersCacheBackingField;
            }
        }

        public static TriArray<TriTypeProcessor> AllTypeProcessors =>
            _allTypeProcessorBackingField ??= (
                from processorType in TypeCache.GetTypesDerivedFrom(typeof(TriTypeProcessor))
                let attr = processorType.GetCustomAttribute<RegisterTriTypeProcessorAttribute>()
                where attr != null
                orderby attr.Order
                select (TriTypeProcessor) Activator.CreateInstance(processorType)
            ).ToList();

        public static TriArray<Info<RegisterTriValueDrawerAttribute>> AllValueDrawerTypes =>
            _allValueDrawerTypesBackingField ??= (
                from drawerType in TypeCache.GetTypesDerivedFrom(typeof(TriValueDrawer))
                let attr = drawerType.GetCustomAttribute<RegisterTriValueDrawerAttribute>()
                where attr != null
                where ValueDrawerMatcher.Match(drawerType)
                select new Info<RegisterTriValueDrawerAttribute>(drawerType, attr)
            ).ToList();

        public static TriArray<Info<RegisterTriAttributeDrawerAttribute>> AllAttributeDrawerTypes =>
            _allAttributeDrawerTypesBackingField ??= (
                from drawerType in TypeCache.GetTypesDerivedFrom(typeof(TriAttributeDrawer))
                let attr = drawerType.GetCustomAttribute<RegisterTriAttributeDrawerAttribute>()
                where attr != null && AttributeDrawerMatcher.Match(drawerType)
                select new Info<RegisterTriAttributeDrawerAttribute>(drawerType, attr)
            ).ToList();

        public static TriArray<Info<RegisterTriValueValidatorAttribute>> AllValueValidatorTypes =>
            _allValueValidatorTypesBackingField ??= (
                from validatorType in TypeCache.GetTypesDerivedFrom(typeof(TriValueValidator))
                let attr = validatorType.GetCustomAttribute<RegisterTriValueValidatorAttribute>()
                where attr != null && ValueValidatorMatcher.Match(validatorType)
                select new Info<RegisterTriValueValidatorAttribute>(validatorType, attr)
            ).ToList();

        public static TriArray<Info<RegisterTriAttributeValidatorAttribute>> AllAttributeValidatorTypes =>
            _allAttributeValidatorTypesBackingField ??= (
                from validatorType in TypeCache.GetTypesDerivedFrom(typeof(TriAttributeValidator))
                let attr = validatorType.GetCustomAttribute<RegisterTriAttributeValidatorAttribute>()
                where attr != null && AttributeValidatorMatcher.Match(validatorType)
                select new Info<RegisterTriAttributeValidatorAttribute>(validatorType, attr)
            ).ToList();

        public static TriArray<Info<RegisterTriPropertyHideProcessor>> AllHideProcessors =>
            _allHideProcessorTypesBackingField ??= (
                from processorType in TypeCache.GetTypesDerivedFrom(typeof(TriPropertyHideProcessor))
                let attr = processorType.GetCustomAttribute<RegisterTriPropertyHideProcessor>()
                where attr != null && HideProcessorMatcher.Match(processorType)
                select new Info<RegisterTriPropertyHideProcessor>(processorType, attr)
            ).ToList();

        public static TriArray<Info<RegisterTriPropertyDisableProcessor>> AllDisableProcessors =>
            _allDisableProcessorTypesBackingField ??= (
                from processorType in TypeCache.GetTypesDerivedFrom(typeof(TriPropertyDisableProcessor))
                let attr = processorType.GetCustomAttribute<RegisterTriPropertyDisableProcessor>()
                where attr != null && DisableProcessorMatcher.Match(processorType)
                select new Info<RegisterTriPropertyDisableProcessor>(processorType, attr)
            ).ToList();

        public static TriPropertyCollectionVisualElement TryCreateGroupVisualElementFor(
            DeclareGroupBaseAttribute attribute)
        {
            if (!AllGroupDrawersCache.TryGetValue(attribute.GetType(), out var attr))
            {
                return null;
            }

            return attr.CreateVisualElementInternal(attribute);
        }

        public static void CreateValueDrawersFor(ref List<TriCustomDrawer> result, Type valueType)
        {
            foreach (var drawer in AllValueDrawerTypes)
            {
                if (ValueDrawerMatcher.Match(drawer.DrawerType, valueType))
                {
                    result ??= new List<TriCustomDrawer>();
                    result.Add(CreateInstance<TriValueDrawer>(drawer.DrawerType, valueType, it =>
                    {
                        it.ApplyOnArrayElement = drawer.Attr.ApplyOnArrayElement;
                        it.Order = drawer.Attr.Order;
                    }));
                }
            }
        }

        public static void CreateAttributeDrawersFor(ref List<TriCustomDrawer> result,
            Type valueType, TriArray<Attribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                foreach (var drawer in AllAttributeDrawerTypes)
                {
                    if (AttributeDrawerMatcher.Match(drawer.DrawerType, attribute.GetType()))
                    {
                        result ??= new List<TriCustomDrawer>();
                        result.Add(CreateInstance<TriAttributeDrawer>(drawer.DrawerType, valueType, it =>
                        {
                            it.ApplyOnArrayElement = drawer.Attr.ApplyOnArrayElement;
                            it.Order = drawer.Attr.Order;
                            it.RawAttribute = attribute;
                        }));
                    }
                }
            }
        }

        public static void CreateValueValidatorsFor(ref List<TriValidator> result, Type valueType)
        {
            foreach (var validator in AllValueValidatorTypes)
            {
                if (ValueValidatorMatcher.Match(validator.DrawerType, valueType))
                {
                    result ??= new List<TriValidator>();
                    result.Add(CreateInstance<TriValueValidator>(validator.DrawerType, valueType,
                        it =>
                        {
                            it.ApplyOnArrayElement = validator.Attr.ApplyOnArrayElement;
                        }));
                }
            }
        }

        public static void CreateAttributeValidatorsFor(ref List<TriValidator> result,
            Type valueType, TriArray<Attribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                foreach (var validator in AllAttributeValidatorTypes)
                {
                    if (AttributeValidatorMatcher.Match(validator.DrawerType, attribute.GetType()))
                    {
                        result ??= new List<TriValidator>();
                        result.Add(CreateInstance<TriAttributeValidator>(validator.DrawerType, valueType, it =>
                        {
                            it.ApplyOnArrayElement = validator.Attr.ApplyOnArrayElement;
                            it.RawAttribute = attribute;
                        }));
                    }
                }
            }
        }

        public static void CreateHideProcessorsFor(ref List<TriPropertyHideProcessor> result,
            Type valueType, TriArray<Attribute> attributes)
        {
            foreach (var processor in AllHideProcessors)
            {
                foreach (var attribute in attributes)
                {
                    if (HideProcessorMatcher.Match(processor.DrawerType, attribute.GetType()))
                    {
                        result ??= new List<TriPropertyHideProcessor>();
                        result.Add(CreateInstance<TriPropertyHideProcessor>(
                            processor.DrawerType, valueType, it =>
                            {
                                it.ApplyOnArrayElement = processor.Attr.ApplyOnArrayElement;
                                it.RawAttribute = attribute;
                            }));
                    }
                }
            }
        }

        public static void CreateDisableProcessorsFor(ref List<TriPropertyDisableProcessor> result,
            Type valueType, TriArray<Attribute> attributes)
        {
            foreach (var processor in AllDisableProcessors)
            {
                foreach (var attribute in attributes)
                {
                    if (DisableProcessorMatcher.Match(processor.DrawerType, attribute.GetType()))
                    {
                        result ??= new List<TriPropertyDisableProcessor>();
                        result.Add(CreateInstance<TriPropertyDisableProcessor>(
                            processor.DrawerType, valueType, it =>
                            {
                                it.ApplyOnArrayElement = processor.Attr.ApplyOnArrayElement;
                                it.RawAttribute = attribute;
                            }));
                    }
                }
            }
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

        private class GenericTypeMatcher
        {
            private readonly Dictionary<Type, (bool, Type)> _cache = new Dictionary<Type, (bool, Type)>();
            private readonly Type _expectedGenericType;

            private GenericTypeMatcher(Type expectedGenericType)
            {
                _expectedGenericType = expectedGenericType;
            }

            public static implicit operator GenericTypeMatcher(Type expectedGenericType)
            {
                return new GenericTypeMatcher(expectedGenericType);
            }

            public bool Match(Type type, Type targetType)
            {
                return MatchOut(type, out var constraint) &&
                       constraint.IsAssignableFrom(targetType);
            }

            public bool Match(Type type)
            {
                return MatchOut(type, out _);
            }

            public bool MatchOut(Type type, out Type targetType)
            {
                if (_cache.TryGetValue(type, out var cachedResult))
                {
                    targetType = cachedResult.Item2;
                    return cachedResult.Item1;
                }

                var succeed = MatchInternal(type, out targetType);
                _cache[type] = (succeed, targetType);
                return succeed;
            }

            private bool MatchInternal(Type type, out Type targetType)
            {
                targetType = null;

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

                Type genericArgConstraints = null;
                if (type.IsGenericType)
                {
                    var genericArg = type.GetGenericArguments().SingleOrDefault();

                    if (genericArg == null ||
                        genericArg.GenericParameterAttributes != GenericParameterAttributes.None)
                    {
                        Debug.LogError(
                            $"{type.Name} must contains only one generic arg with simple constant e.g. <where T : bool>");
                        return false;
                    }

                    genericArgConstraints = genericArg.GetGenericParameterConstraints().SingleOrDefault();
                }

                var drawerType = type.BaseType;

                while (drawerType != null)
                {
                    if (drawerType.IsGenericType &&
                        drawerType.GetGenericTypeDefinition() == _expectedGenericType)
                    {
                        targetType = drawerType.GetGenericArguments()[0];

                        if (targetType.IsGenericParameter)
                        {
                            if (genericArgConstraints == null)
                            {
                                Debug.LogError(
                                    $"{type.Name} must contains only one generic arg with simple constant e.g. <where T : bool>");
                                return false;
                            }

                            targetType = genericArgConstraints;
                        }

                        return true;
                    }

                    drawerType = drawerType.BaseType;
                }

                Debug.LogError($"{type.Name} must implement {_expectedGenericType}");
                return false;
            }
        }

        public struct Info<TAttr> where TAttr : Attribute
        {
            public Info(Type drawerType, TAttr attr)
            {
                DrawerType = drawerType;
                Attr = attr;
            }

            public Type DrawerType { get; }
            public TAttr Attr { get; }
        }
    }
}