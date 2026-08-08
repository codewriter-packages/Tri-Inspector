using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(LayerDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class LayerDrawer : TriAttributeDrawer<LayerAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var type = propertyDefinition.FieldType;
            if (type != typeof(int))
            {
                return "Layer attribute can only be used on field with int type";
            }

            return base.Initialize(propertyDefinition);
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var field = new LayerField();

            return TriBuiltinFieldFactory.CreateForProperty(property, field,
                () => (int) property.Value,
                value => property.SetValue(value));
        }
    }
}
