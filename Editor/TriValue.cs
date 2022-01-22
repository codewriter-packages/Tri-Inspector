namespace TriInspector
{
    public struct TriValue<T>
    {
        internal TriValue(TriProperty property)
        {
            Property = property;
        }

        public TriProperty Property { get; }

        public T Value
        {
            get => (T) Property.Value;
            set => Property.SetValue(value);
        }
    }
}