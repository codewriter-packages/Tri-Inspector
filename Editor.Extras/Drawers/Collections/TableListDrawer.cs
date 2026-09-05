using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.VisualElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(TableListDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class TableListDrawer : TriAttributeDrawer<TableListAttribute>
    {
        private ValueResolver<string>[] _headerResolvers;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.IsArray || propertyDefinition.IsDictionary)
            {
                return "[TableList] valid only on lists";
            }

            if (Attribute.Labels != null)
            {
                _headerResolvers = new ValueResolver<string>[Attribute.Labels.Length];
                for (var i = 0; i < Attribute.Labels.Length; i++)
                {
                    _headerResolvers[i] = Attribute.Labels[i] != null
                        ? ValueResolver.ResolveString(propertyDefinition, Attribute.Labels[i])
                        : null;
                }

                foreach (var resolver in _headerResolvers)
                {
                    if (ValueResolver.TryGetErrorString(resolver, out var error))
                    {
                        return error;
                    }
                }
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriTableListVisualElement(property, _headerResolvers);
        }
    }
}
