using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(ObjectReferenceDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class ObjectReferenceDrawer : TriValueDrawer<Object>
    {
        public override TriElement CreateElement(TriValue<Object> value, TriElement next)
        {
            if (value.Property.IsRootProperty || value.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return new TriObjectReferenceElement(value);
        }
    }
}
