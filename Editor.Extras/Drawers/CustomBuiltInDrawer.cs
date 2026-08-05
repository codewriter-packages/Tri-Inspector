using TriInspector;
using TriInspector.Drawers;
using TriInspector.Utilities;
using TriInspector.VisualElements;
using TriInspectorUnityInternalBridge;
using UnityEngine.UIElements;

[assembly: RegisterTriValueDrawer(typeof(CustomBuiltInDrawer), TriDrawerOrder.Fallback - 999)]

namespace TriInspector.Drawers
{
    public class CustomBuiltInDrawer : TriValueDrawer<object>
    {
        public override VisualElement CreateVisualElement(TriValue<object> propertyValue, VisualElement next)
        {
            if (propertyValue.Property.IsRootProperty)
            {
                return next;
            }

            var property = propertyValue.Property;

            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                var handler = ScriptAttributeUtilityProxy.GetHandler(serializedProperty);

                var drawWithHandler = handler.hasPropertyDrawer ||
                                      property.PropertyType == TriPropertyType.Primitive ||
                                      TriUnityInspectorUtilities.MustDrawWithUnity(property);

                if (drawWithHandler)
                {
                    return new TriBuiltInPropertyVisualElement(property, serializedProperty);
                }
            }

            return next;
        }
    }
}