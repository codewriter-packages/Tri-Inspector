using TriInspector;
using TriInspector.Processors;
using TriInspector.Resolvers;

[assembly: RegisterTriPropertyDisableProcessor(typeof(DisableIfProcessor))]
[assembly: RegisterTriPropertyDisableProcessor(typeof(EnableIfProcessor))]

namespace TriInspector.Processors
{
    public abstract class DisableIfProcessorBase<T> : TriPropertyDisableProcessor<T>
        where T : ConditionalDisableBaseAttribute
    {
        private readonly bool _inverse;

        private ValueResolver<object> _conditionResolver;

        protected DisableIfProcessorBase(bool inverse)
        {
            _inverse = inverse;
        }

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

        public sealed override bool IsDisabled(TriProperty property)
        {
            var val = _conditionResolver.GetValue(property);
            var equal = val?.Equals(Attribute.Value) ?? Attribute.Value == null;
            return equal != _inverse;
        }
    }

    public sealed class DisableIfProcessor : DisableIfProcessorBase<DisableIfAttribute>
    {
        public DisableIfProcessor() : base(false)
        {
        }
    }

    public sealed class EnableIfProcessor : DisableIfProcessorBase<EnableIfAttribute>
    {
        public EnableIfProcessor() : base(true)
        {
        }
    }
}