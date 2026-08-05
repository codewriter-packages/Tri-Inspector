using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriValueDrawer(typeof(ObjectReferenceDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class ObjectReferenceDrawer : TriValueDrawer<Object>
    {
        public override VisualElement CreateVisualElement(TriValue<Object> value, VisualElement next)
        {
            if (value.Property.IsRootProperty || value.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return new TriObjectReferenceVisualElement(value);
        }
    }
}
