using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(PropertyTextAreaDrawer), TriDrawerOrder.Decorator,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
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

            return TriBuiltinFieldFactory.CreateForProperty(property, field,
                () => (string) property.Value ?? "",
                value => property.SetValue(value));
        }
    }
}