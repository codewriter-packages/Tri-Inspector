namespace TriInspector
{
    public abstract class ConditionalDisableAttribute : DisableBaseAttribute
    {
        protected ConditionalDisableAttribute(string condition)
        {
            Condition = condition;
        }

        public string Condition { get; }
    }
}