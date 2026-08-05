using TriInspector.VisualElements;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    public abstract class BuiltinDrawerBase<T> : TriValueDrawer<T>
    {
        public override VisualElement CreateVisualElement(TriValue<T> propertyValue, VisualElement next)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            var field = CreateField();
            if (field == null)
            {
                return next;
            }

            return TriBuiltinFieldFactory.Create(propertyValue, field);
        }

        protected virtual BaseField<T> CreateField()
        {
            return null;
        }
    }
}