using System;
using System.Collections.Generic;
using TriInspector;
using TriInspector.TypeProcessors;

[assembly: RegisterTriTypeProcessor(typeof(TriSortPropertiesTypeProcessor), 10000)]

namespace TriInspector.TypeProcessors
{
    public class TriSortPropertiesTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            properties.Sort(PropertyOrderComparer.Instance);
        }

        private class PropertyOrderComparer : IComparer<TriPropertyDefinition>
        {
            public static readonly PropertyOrderComparer Instance = new PropertyOrderComparer();

            public int Compare(TriPropertyDefinition x, TriPropertyDefinition y)
            {
                return x.Order.CompareTo(y.Order);
            }
        }
    }
}