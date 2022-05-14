namespace TriInspector
{
    public abstract class ConditionalDisableBaseAttribute : DisableBaseAttribute
    {
        protected ConditionalDisableBaseAttribute(string condition, object value)
        {
            Condition = condition;
            Value = value;
        }

        public string Condition { get; }
        public object Value { get; }
    }
}