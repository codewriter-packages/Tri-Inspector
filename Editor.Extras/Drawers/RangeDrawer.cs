using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
    public class RangeDrawer : TriAttributeDrawer<RangeAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition definition)
        {
            if (definition.FieldType != typeof(int) && definition.FieldType != typeof(float))
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

            if (property.FieldType == typeof(int))
            {
                var field = new SliderInt((int) Attribute.min, (int) Attribute.max)
                {
                    showInputField = true,
                };
                field.BindTri(property);
                return field;
            }

            var floatField = new Slider(Attribute.min, Attribute.max)
            {
                showInputField = true,
            };
            floatField.BindTri(property);
            return floatField;
        }
    }
}
