using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[assembly: RegisterTriAttributeDrawer(typeof(InlineEditorDrawer), TriDrawerOrder.Decorator,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class InlineEditorDrawer : TriAttributeDrawer<InlineEditorAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!typeof(Object).IsAssignableFrom(propertyDefinition.FieldType))
            {
                return "[InlineEditor] valid only on Object fields";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriInlineEditorVisualElement(property, new TriInlineEditorVisualElement.Props
            {
                mode = Attribute.Mode,
                previewHeight = Attribute.PreviewHeight,
            });
        }
    }
}