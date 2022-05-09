using System;

namespace TriInspector.Resolvers
{
    internal class ErrorMethodResolver<TReturnType> : MethodResolver<TReturnType>
    {
        private readonly string _method;

        public ErrorMethodResolver(TriPropertyDefinition propertyDefinition, string method)
        {
            _method = method;
        }

        public override bool TryGetErrorString(out string error)
        {
            error = $"Method '{_method}' not exists or has wrong signature";
            return true;
        }

        public override TReturnType InvokeForTarget(TriProperty property, int targetIndex)
        {
            throw new InvalidOperationException("Method not exists");
        }
    }
}