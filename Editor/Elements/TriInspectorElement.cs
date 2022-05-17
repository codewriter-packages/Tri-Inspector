using System;
using System.Collections.Generic;

namespace TriInspector.Elements
{
    internal class TriInspectorElement : TriPropertyCollectionBaseElement
    {
        public TriInspectorElement(Type targetObjectType, IReadOnlyList<TriProperty> properties)
        {
            DeclareGroups(targetObjectType);

            foreach (var childProperty in properties)
            {
                AddProperty(childProperty);
            }
        }
    }
}