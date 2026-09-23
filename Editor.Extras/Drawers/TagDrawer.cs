using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
    public class TagDrawer : TriAttributeDrawer<TagAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition definition)
        {
            var type = definition.FieldType;
            if (type != typeof(string))
            {
                return "Tag attribute can only be used on field with string type";
            }
            return base.Initialize(definition);
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var field = new TagField();
            field.BindTri(property);
            return field;
        }
    }
}
