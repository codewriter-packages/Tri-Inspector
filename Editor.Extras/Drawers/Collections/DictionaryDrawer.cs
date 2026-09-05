using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(DictionaryDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class DictionaryDrawer : TriAttributeDrawer<DictionaryDrawerSettings>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.IsDictionary)
            {
                return "[DictionaryDrawerSettings] valid only on dictionaries";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            if (property.PropertyTree.TargetsCount != 1)
            {
                return new TriInfoBoxVisualElement("Dictionary multi-object editing is not supported",
                    TriMessageType.Info);
            }

            switch (Attribute.Layout)
            {
                case DictionaryLayout.OneColumnWithValueFoldout:
                    return new TriDictionaryOneColumnVisualElement(property, valueVisible: false);

                case DictionaryLayout.OneColumnWithValueVisible:
                    return new TriDictionaryOneColumnVisualElement(property, valueVisible: true);

                case DictionaryLayout.TwoColumns:
                default:
                    return new TriTableListVisualElement(property);
            }
        }

        public class TriDictionaryOneColumnVisualElement : TriCollectionVisualElement
        {
            private readonly bool _valueVisible;

            public TriDictionaryOneColumnVisualElement(TriProperty property, bool valueVisible) : base(property)
            {
                _valueVisible = valueVisible;
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
        }
    }
}