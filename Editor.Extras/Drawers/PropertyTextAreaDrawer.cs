using TriInspector.VisualElements;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
    public class PropertyTextAreaDrawer : TriAttributeDrawer<PropertyTextAreaAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var type = propertyDefinition.FieldType;
            if (type != typeof(string))
            {
                return "PropertyTextArea attribute can only be used on field";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var field = new TextField
            {
                multiline = true,
            };

            field.BindTri(property, v => v ?? "", v => v);
            return field;
        }
    }
}