using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
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
            field.BindTri(property);
            return field;
        }
    }
}