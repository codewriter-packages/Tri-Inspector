using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(TableListDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class TableListDrawer : TriAttributeDrawer<TableListAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.IsArray || propertyDefinition.IsDictionary)
            {
                return "[TableList] valid only on lists";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            return new TriTableListVisualElement(property);
        }
    }
}