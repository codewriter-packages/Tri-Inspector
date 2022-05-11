using JetBrains.Annotations;

namespace TriInspector.Resolvers
{
    public static class ValueResolver
    {
        public static ValueResolver<T> Resolve<T>(TriPropertyDefinition propertyDefinition,
            string expression)
        {
            if (InstanceFieldValueResolver<T>.TryResolve(propertyDefinition, expression, out var ifr))
            {
                return ifr;
            }

            if (InstancePropertyValueResolver<T>.TryResolve(propertyDefinition, expression, out var ipr))
            {
                return ipr;
            }

            if (InstanceMethodValueResolver<T>.TryResolve(propertyDefinition, expression, out var imr))
            {
                return imr;
            }

            return new ErrorValueResolver<T>(propertyDefinition, expression);
        }

        public static ValueResolver<string> ResolveString(TriPropertyDefinition propertyDefinition,
            string expression)
        {
            if (expression != null && expression.StartsWith("$"))
            {
                return Resolve<string>(propertyDefinition, expression.Substring(1));
            }

            return new ConstantValueResolver<string>(expression);
        }
    }

    public abstract class ValueResolver<T>
    {
        [PublicAPI]
        public abstract bool TryGetErrorString(out string error);

        [PublicAPI]
        public abstract T GetValue(TriProperty property, T defaultValue = default);
    }

    public sealed class ConstantValueResolver<T> : ValueResolver<T>
    {
        private readonly T _value;

        public ConstantValueResolver(T value)
        {
            _value = value;
        }

        public override bool TryGetErrorString(out string error)
        {
            error = default;
            return false;
        }

        public override T GetValue(TriProperty property, T defaultValue = default)
        {
            return _value;
        }
    }
}