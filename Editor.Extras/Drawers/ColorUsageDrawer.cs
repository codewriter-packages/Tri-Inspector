using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
    public class ColorUsageDrawer : TriAttributeDrawer<ColorUsageAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition definition)
        {
            if (definition.FieldType != typeof(Color))
            {
                return TriExtensionInitializationResult.Skip;
            }

            return base.Initialize(definition);
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            if (property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            var field = new ColorField
            {
                showAlpha = Attribute.showAlpha,
                hdr = Attribute.hdr,
            };
            field.BindTri(property);
            return field;
        }
    }
}
