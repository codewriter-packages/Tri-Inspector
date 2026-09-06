using TriInspector.Utilities;
using TriInspector.VisualElements;
using TriInspectorUnityInternalBridge;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriValueDrawer(TriDrawerOrder.Fallback - 999)]
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
                                      (property.PropertyType == TriPropertyType.Primitive && next is TriNoDrawerVisualElement) ||
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