using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(DictionaryDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class DictionaryDrawer : TriAttributeDrawer<DictionaryDrawerSettings>
    {
        private ValueResolver<string>[] _headerResolvers;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.IsDictionary)
            {
                return "[DictionaryDrawerSettings] valid only on dictionaries";
            }

            var keyResolver = Attribute.KeyLabel != null
                ? ValueResolver.ResolveString(propertyDefinition, Attribute.KeyLabel)
                : null;
            var valueResolver = Attribute.ValueLabel != null
                ? ValueResolver.ResolveString(propertyDefinition, Attribute.ValueLabel)
                : null;

            if (ValueResolver.TryGetErrorString(keyResolver, valueResolver, out var error))
            {
                return error;
            }

            _headerResolvers = new[] {keyResolver, valueResolver,};

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return CreateElement(property, _headerResolvers, Attribute.Layout);
        }

        public static VisualElement CreateElement(TriProperty property,
            ValueResolver<string>[] headerResolvers, DictionaryLayout layout)
        {
            if (property.PropertyTree.TargetsCount != 1)
            {
                return new TriInfoBoxVisualElement("Dictionary multi-object editing is not supported",
                    TriMessageType.Info);
            }

            switch (layout)
            {
                case DictionaryLayout.OneColumnWithValueFoldout:
                    return new TriDictionaryOneColumnVisualElement(property, valueVisible: false, headerResolvers);

                case DictionaryLayout.OneColumnWithValueVisible:
                    return new TriDictionaryOneColumnVisualElement(property, valueVisible: true, headerResolvers);

                case DictionaryLayout.TwoColumns:
                default:
                    return new TriTableListVisualElement(property, headerResolvers);
            }
        }

        public class TriDictionaryOneColumnVisualElement : TriCollectionVisualElement
        {
            private readonly bool _valueVisible;

            public TriDictionaryOneColumnVisualElement(TriProperty property, bool valueVisible,
                ValueResolver<string>[] headerResolvers = null) : base(property)
            {
                _valueVisible = valueVisible;

                if (headerResolvers != null)
                {
                    var labelOverride = new DictionaryLabelOverrideContext(property, headerResolvers);
                    RegisterCallback<AttachToPanelEvent>(_ =>
                        property.PropertyTree.AddPropertyOverride(labelOverride));
                    RegisterCallback<DetachFromPanelEvent>(_ =>
                        property.PropertyTree.RemovePropertyOverride(labelOverride));
                }
            }

            protected override VisualElement CreateItemElement(TriProperty property)
            {
                var row = new VisualElement();

                foreach (var child in property.ChildrenProperties)
                {
                    row.Add(CreateCell(child));
                }

                return new TriValidationResultsVisualElement(property, row);
            }

            private VisualElement CreateCell(TriProperty property)
            {
                return new TriPropertyVisualElement(property, new TriPropertyVisualElement.Props
                {
                    forceInline = _valueVisible,
                });
            }

            private class DictionaryLabelOverrideContext : TriPropertyOverrideContext
            {
                private readonly TriProperty _listProperty;
                private readonly ValueResolver<string> _keyResolver;
                private readonly ValueResolver<string> _valueResolver;
                private readonly GUIContent _keyLabel = new GUIContent();
                private readonly GUIContent _valueLabel = new GUIContent();

                public DictionaryLabelOverrideContext(TriProperty listProperty, ValueResolver<string>[] resolvers)
                {
                    _listProperty = listProperty;
                    _keyResolver = resolvers[0];
                    _valueResolver = resolvers[1];
                }

                public override bool TryGetDisplayName(TriProperty property, out GUIContent displayName)
                {
                    if (property.Parent == null || property.Parent.Parent != _listProperty)
                    {
                        displayName = null;
                        return false;
                    }

                    var children = property.Parent.ChildrenProperties;

                    if (children.Count > 0 && children[0] == property)
                    {
                        var label = _keyResolver?.GetValue(_listProperty, null);
                        if (label != null)
                        {
                            _keyLabel.text = label;
                            displayName = _keyLabel;
                            return true;
                        }
                    }
                    else if (children.Count > 1 && children[1] == property)
                    {
                        var label = _valueResolver?.GetValue(_listProperty, null);
                        if (label != null)
                        {
                            _valueLabel.text = label;
                            displayName = _valueLabel;
                            return true;
                        }
                    }

                    displayName = null;
                    return false;
                }
            }
        }
    }
}