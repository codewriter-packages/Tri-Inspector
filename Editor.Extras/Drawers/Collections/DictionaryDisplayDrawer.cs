#if UNITY_6000_6_OR_NEWER

using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(DictionaryDisplayDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class DictionaryDisplayDrawer : TriAttributeDrawer<DictionaryDisplayAttribute>
    {
        private ValueResolver<string>[] _headerResolvers;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.IsDictionary)
            {
                return "[DictionaryDisplay] valid only on dictionaries";
            }

            var keyResolver = Attribute.keyLabel != null
                ? new ConstantValueResolver<string>(Attribute.keyLabel)
                : null;
            var valueResolver = Attribute.valueLabel != null
                ? new ConstantValueResolver<string>(Attribute.valueLabel)
                : null;

            if (ValueResolver.TryGetErrorString(keyResolver, valueResolver, out var error))
            {
                return error;
            }

            _headerResolvers = new[] {keyResolver, valueResolver};

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return DictionaryDrawer.CreateElement(property, _headerResolvers, Attribute.layout);
        }
    }
}

#endif