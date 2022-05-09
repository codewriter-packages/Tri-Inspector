namespace TriInspector.Resolvers
{
    public static class MethodResolver
    {
        public static MethodResolver<TReturnType> Resolve<TReturnType>(TriPropertyDefinition propertyDefinition,
            string method)
        {
            if (InstanceMethodResolver<TReturnType>.TryResolve(propertyDefinition, method, out var imr))
            {
                return imr;
            }

            return new ErrorMethodResolver<TReturnType>(propertyDefinition, method);
        }
    }

    public abstract class MethodResolver<TReturnType>
    {
        public abstract bool TryGetErrorString(out string error);

        public abstract TReturnType InvokeForTarget(TriProperty property, int targetIndex);
    }
}