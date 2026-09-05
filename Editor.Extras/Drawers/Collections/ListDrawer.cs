using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(ListDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
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