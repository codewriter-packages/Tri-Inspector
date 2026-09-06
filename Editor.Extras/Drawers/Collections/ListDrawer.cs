using TriInspector.VisualElements;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer)]
    public class ListDrawer : TriAttributeDrawer<ListDrawerSettingsAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.IsArray || propertyDefinition.IsDictionary)
            {
                return "[ListDrawerSettings] valid only on lists";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriListVisualElement(property, inlineElements: !Attribute.ShowElementLabels);
        }
    }
}