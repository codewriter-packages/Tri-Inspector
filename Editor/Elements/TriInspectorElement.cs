namespace TriInspector.Elements
{
    internal class TriInspectorElement : TriPropertyCollectionBaseElement
    {
        public TriInspectorElement(TriPropertyTree propertyTree)
        {
            DeclareGroups(propertyTree.TargetObjectType);

            foreach (var childProperty in propertyTree.Properties)
            {
                AddProperty(childProperty);
            }
        }
    }
}