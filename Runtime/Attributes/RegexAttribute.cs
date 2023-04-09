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
        
        public bool PreviewExpression { get; set; }
        public bool DynamicExpression { get; set; }
        public string Expression { get; }
    }
}