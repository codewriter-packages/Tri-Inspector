using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEditor.UIElements;
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

            return new TriAlignedLabelVisualElement(value.Property, new TriObjectReference(value));
        }

        private class TriObjectReference : ObjectField
        {
            public TriObjectReference(TriValue<Object> value)
            {
                objectType = value.Property.FieldType;
                allowSceneObjects = value.Property.PropertyTree.TargetIsPersistent == false;

                this.RegisterValueChangedCallback(evt => value.SetValue(evt.newValue));
                this.AutoSyncValueFromProperty(value.Property);
            }
        }
    }
}