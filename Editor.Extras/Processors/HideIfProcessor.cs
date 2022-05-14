using TriInspector;
using TriInspector.Processors;
using TriInspector.Resolvers;

[assembly: RegisterTriPropertyHideProcessor(typeof(HideIfProcessor))]
[assembly: RegisterTriPropertyHideProcessor(typeof(ShowIfProcessor))]

namespace TriInspector.Processors
{
    public abstract class HideIfProcessorBase<T> : TriPropertyHideProcessor<T>
        where T : ConditionalHideBaseAttribute
    {
        private readonly bool _inverse;

        private ValueResolver<object> _conditionResolver;

        protected HideIfProcessorBase(bool inverse)
        {
            _inverse = inverse;
        }

        public override void Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _conditionResolver = ValueResolver.Resolve<object>(propertyDefinition, Attribute.Condition);
        }

        public sealed override bool IsHidden(TriProperty property)
        {
            var val = _conditionResolver.GetValue(property);
            var equal = val?.Equals(Attribute.Value) ?? Attribute.Value == null;
            return equal != _inverse;
        }
    }

    public sealed class HideIfProcessor : HideIfProcessorBase<HideIfAttribute>
    {
        public HideIfProcessor() : base(false)
        {
        }
    }

    public sealed class ShowIfProcessor : HideIfProcessorBase<ShowIfAttribute>
    {
        public ShowIfProcessor() : base(true)
        {
        }
    }
}