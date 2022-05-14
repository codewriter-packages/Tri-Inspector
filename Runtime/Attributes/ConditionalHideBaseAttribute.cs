namespace TriInspector
{
    public abstract class ConditionalHideBaseAttribute : HideBaseAttribute
    {
        protected ConditionalHideBaseAttribute(string condition, object value)
        {
            Condition = condition;
            Value = value;
        }

        public string Condition { get; }
        public object Value { get; }
    }
}