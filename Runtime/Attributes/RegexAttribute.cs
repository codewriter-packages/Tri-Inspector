using System;

namespace TriInspector
{
    [AttributeUsage((AttributeTargets.Field | AttributeTargets.Property))]
    public class RegexAttribute : Attribute
    {
        public RegexAttribute(string expression)
        {
            Expression = expression;
        }
        
        public bool DynamicExpression { get; set; }
        public bool PreviewExpression { get; set; }
        public string Example { get; set; }
        public string Expression { get; }
    }
}