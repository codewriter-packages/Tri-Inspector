namespace TriInspector
{
    public abstract class ConditionalHideAttribute : HideBaseAttribute
    {
        protected ConditionalHideAttribute(string condition)
        {
            Condition = condition;
        }

        public string Condition { get; }
    }
}