using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
    public class TextAreaDrawer : TriAttributeDrawer<TextAreaAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition definition)
        {
            if (definition.FieldType != typeof(string))
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

            var field = new TextField
            {
                multiline = true,
            };

            field.BindTri(property, v => v ?? "", v => v);
            return field;
        }
    }
}
