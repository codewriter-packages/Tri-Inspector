using System;
using System.Linq;
using TriInspector;
using TriInspector.Processors;
using TriInspector.Resolvers;

[assembly: RegisterTriPropertyHideProcessor(typeof(HideIfProcessor))]

namespace TriInspector.Processors
{
    public class HideIfProcessor : TriPropertyHideProcessor<HideIfAttribute>
    {
        private ValueResolver<object> _conditionResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _conditionResolver = ValueResolver.Resolve<object>(propertyDefinition, Attribute.Condition);

            if (_conditionResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public sealed override bool IsHidden(TriProperty property)
        {
            var val = _conditionResolver.GetValue(property);

            var equal = Attribute.ConditionType switch
            {
                ConditionType.And => Attribute.Values.All(t => val?.Equals(t) ?? Attribute.Values == null),
                ConditionType.Or => Attribute.Values.Any(t => val?.Equals(t) ?? Attribute.Values == null),
                ConditionType.AndNot => !Attribute.Values.All(t => val?.Equals(t) ?? Attribute.Values == null),
                ConditionType.OrNot => !Attribute.Values.Any(t => val?.Equals(t) ?? Attribute.Values == null),
                _ => throw new ArgumentOutOfRangeException(),
            };

            return equal != Attribute.Inverse;
        }
    }
}