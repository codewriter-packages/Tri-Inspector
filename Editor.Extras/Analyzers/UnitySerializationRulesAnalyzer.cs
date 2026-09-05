using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Editor.Extras.Analyzers;
using TriInspector;
using TriInspector.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[assembly: RegisterTriTypeProcessor(typeof(UnitySerializationRulesAnalyzer), 999999)]
[assembly: RegisterTriAttributeValidator(typeof(UnitySerializationRuleValidator))]
[assembly: RegisterTriAttributeDrawer(typeof(UnitySerializationRuleDrawer), TriDrawerOrder.Drawer + 1)]

namespace Editor.Extras.Analyzers
{
    internal class UnitySerializationRulesAnalyzer : TriTypeProcessor
    {
        private static readonly UnitySerializationRule[] Rules =
        {
            new("UAC1000", "SerializeReference field type missing the Serializable attribute",
                UnitySerializationRuleTarget.SerializeReference, TriMessageType.Warning,
                p => p.myType.IsClass && p.myType != typeof(object) &&
                     !TriUnitySerializationUtilities.IsTypeSerializableByUnity(p.myType)),

            new("UAC1001", "Public field skipped by serialization due to missing the Serializable attribute",
                UnitySerializationRuleTarget.Public, TriMessageType.Warning,
                p => (p.myType.IsClass || (p.myType.IsValueType && !p.myType.IsPrimitive && !p.myType.IsEnum)) &&
                     !TriUnitySerializationUtilities.IsTypeSerializableByUnity(p.myType)),

#if UNITY_6000_6
            new("UAC1002", "Inheritance hierarchy incomplete serialization",
                UnitySerializationRuleTarget.Any, TriMessageType.Warning,
                p =>
                {
                    if (!p.myType.IsClass)
                    {
                        return false;
                    }

                    for (var baseType = p.myType.BaseType;
                         baseType != null && baseType != typeof(object);
                         baseType = baseType.BaseType)
                    {
                        if (IsSystemType(baseType))
                        {
                            break;
                        }

                        if (!TriUnitySerializationUtilities.IsTypeSerializableByUnity(baseType))
                        {
                            return true;
                        }
                    }

                    return false;
                }),
#endif

            new("UAC1003", "[SerializeReference] used on struct",
                UnitySerializationRuleTarget.SerializeReference, TriMessageType.Error,
                p => p.myType.IsValueType && !p.myType.IsPrimitive && !p.myType.IsEnum),

            new("UAC1004", "[SerializeReference] used on primitive or enum type",
                UnitySerializationRuleTarget.SerializeReference, TriMessageType.Warning,
                p => p.myType.IsPrimitive || p.myType.IsEnum),

            new("UAC1009", "Unsupported collection type for serialization",
                UnitySerializationRuleTarget.Any, TriMessageType.Warning,
                p =>
                {
                    if (p.definition.IsDictionary)
                    {
#if UNITY_6000_6
                        return false;
#else
                        return true;
#endif
                    }

                    return IsUnsupportedCollectionType(p.fieldType) ||
                           (p.definition.IsArray && (
                                   IsUnsupportedCollectionType(p.definition.ArrayElementType) ||
                                   IsListType(p.definition.ArrayElementType))
                           );
                }),

            new("UAC1010", "[SerializeField] used on non-serializable type",
                UnitySerializationRuleTarget.SerializeField, TriMessageType.Warning,
                p => (p.myType.IsClass || (p.myType.IsValueType && !p.myType.IsPrimitive && !p.myType.IsEnum)) &&
                     !TriUnitySerializationUtilities.IsTypeSerializableByUnity(p.myType)),

            new("UAC1011", "Enum type exceeds 32-bit size limit",
                UnitySerializationRuleTarget.Any, TriMessageType.Error,
                p => p.myType.IsEnum && p.myType.GetEnumUnderlyingType() is { } t &&
                     (t == typeof(long) || t == typeof(ulong))),

#if UNITY_6000_6
            new("UAC1015", "Dictionary field missing [SerializeField]",
                UnitySerializationRuleTarget.Public, TriMessageType.Warning,
                p => p.definition.IsDictionary),
#endif

            new("UAC1017", "Tuple type not supported by serialization",
                UnitySerializationRuleTarget.Any, TriMessageType.Warning,
                p => p.myType.IsGenericType && typeof(ITuple).IsAssignableFrom(p.myType)),
        };

        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            foreach (var property in properties)
            {
                var myTarget = property.Origin switch
                {
                    TriPropertyOrigin.UnityPublicField => UnitySerializationRuleTarget.Public,
                    TriPropertyOrigin.UnitySerializeField => UnitySerializationRuleTarget.SerializeField,
                    TriPropertyOrigin.UnitySerializeReference => UnitySerializationRuleTarget.SerializeReference,
                    _ => UnitySerializationRuleTarget.None,
                };

                if (myTarget == UnitySerializationRuleTarget.None)
                {
                    continue;
                }

                var myType = property.IsArray ? property.ArrayElementType : property.FieldType;

                if (typeof(Object).IsAssignableFrom(myType))
                {
                    continue;
                }

                foreach (var rule in Rules)
                {
                    if ((rule.target & myTarget) == 0)
                    {
                        continue;
                    }

                    var ctx = new UnitySerializationRuleContext
                    {
                        definition = property,
                        fieldType = property.FieldType,
                        myType = myType,
                    };

                    if (!rule.match(ctx))
                    {
                        continue;
                    }

                    property.GetEditableAttributes().Add(new UnitySerializationRuleAttribute(rule));
                }
            }
        }

#if UNITY_6000_6
        private static bool IsSystemType(Type type)
        {
            var ns = type.Namespace;
            return ns != null && (ns == "System" || ns.StartsWith("System."));
        }
#endif

        private static bool IsUnsupportedCollectionType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetArrayRank() > 1 || type.GetElementType()!.IsArray;
            }

            if (!type.IsGenericType && !"System.Collections".Equals(type.Namespace))
            {
                return false;
            }

            if (type.IsGenericType && "System.Collections.Generic".Equals(type.Namespace))
            {
                return !IsListType(type);
            }

            if ("System.Collections".Equals(type.Namespace))
            {
                if (type == typeof(IEnumerable))
                {
                    return true;
                }

                for (var currentType = type; currentType != null; currentType = currentType.BaseType)
                {
                    foreach (var currentInterface in currentType.GetInterfaces())
                    {
                        if (currentInterface == typeof(IEnumerable) || currentInterface == typeof(ICollection))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
    }

    [Flags]
    internal enum UnitySerializationRuleTarget
    {
        None = 0,
        Public = 1 << 0,
        SerializeField = 1 << 1,
        SerializeReference = 1 << 2,
        Any = Public | SerializeField | SerializeReference,
    }

    internal struct UnitySerializationRuleContext
    {
        public TriPropertyDefinition definition;
        public Type fieldType;
        public Type myType;
    }

    internal class UnitySerializationRule
    {
        public readonly string code;
        public readonly string message;
        public readonly UnitySerializationRuleTarget target;
        public readonly TriMessageType type;
        public readonly Func<UnitySerializationRuleContext, bool> match;

        public UnitySerializationRule(string code, string message, UnitySerializationRuleTarget target,
            TriMessageType type,
            Func<UnitySerializationRuleContext, bool> match)
        {
            this.code = code;
            this.message = message;
            this.target = target;
            this.type = type;
            this.match = match;
        }
    }

    internal class UnitySerializationRuleValidator : TriAttributeValidator<UnitySerializationRuleAttribute>
    {
        private const string HelpUrl = "https://github.com/codewriter-packages/Tri-Inspector/discussions/241";

        public override TriValidationResult Validate(TriProperty property)
        {
            var rule = Attribute.Rule;

            return new TriValidationResult(false, $"<b>{rule.code}</b> by <b>TriInspector</b>: {rule.message}",
                    rule.type)
                .WithFix(() => Application.OpenURL(HelpUrl), "Help");
        }
    }

    internal class UnitySerializationRuleDrawer : TriAttributeDrawer<UnitySerializationRuleAttribute>
    {
        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var root = new VisualElement();
            root.SetEnabled(false);
            root.Add(next);
            return root;
        }
    }

    internal class UnitySerializationRuleAttribute : Attribute
    {
        public UnitySerializationRuleAttribute(UnitySerializationRule rule)
        {
            Rule = rule;
        }

        public UnitySerializationRule Rule { get; }
    }
}