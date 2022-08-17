using System;
using System.Collections.Generic;

namespace TriInspector
{
    public abstract class TriTypeProcessor
    {
        internal int Order { get; set; }

        public virtual void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
        }
    }
}