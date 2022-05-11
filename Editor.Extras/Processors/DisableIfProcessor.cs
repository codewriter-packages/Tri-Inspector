using TriInspector;
using TriInspector.Processors;
using TriInspector.Resolvers;

[assembly: RegisterTriPropertyDisableProcessor(typeof(DisableIfProcessor))]
[assembly: RegisterTriPropertyDisableProcessor(typeof(EnableIfProcessor))]

namespace TriInspector.Processors
{
    public abstract class DisableIfProcessorBase<T> : TriPropertyDisableProcessor<T>
        where T : ConditionalDisableAttribute
    {
        private readonly bool _inverse;

        private ValueResolver<bool> _conditionResolver;

        protected DisableIfProcessorBase(bool inverse)
        {
            _inverse = inverse;
        }

        public override void Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _conditionResolver = ValueResolver.Resolve<bool>(propertyDefinition, Attribute.Condition);
        }

        public sealed override bool IsDisabled(TriProperty property)
        {
            return _conditionResolver.GetValue(property) != _inverse;
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