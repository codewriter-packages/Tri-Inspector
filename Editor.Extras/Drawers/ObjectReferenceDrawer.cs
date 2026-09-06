using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriValueDrawer(TriDrawerOrder.Fallback)]
    public class ObjectReferenceDrawer : TriValueDrawer<Object>
    {
        public override VisualElement CreateVisualElement(TriValue<Object> value, VisualElement next)
        {
            if (value.Property.IsRootProperty)
            {
                return next;
            }

            return new TriObjectReference(value);
        }

        private class TriObjectReference : ObjectField
        {
            public TriObjectReference(TriValue<Object> value)
            {
                objectType = value.Property.FieldType;
                allowSceneObjects = value.Property.PropertyTree.TargetIsPersistent == false;

                this.BindTri(value.Property);
            }
        }
    }
}