using TriInspector.VisualElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Drawer, ApplyOnArrayElement = true)]
    public class MinDrawer : TriAttributeDrawer<MinAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition definition)
        {
            if (definition.FieldType != typeof(int) && definition.FieldType != typeof(float))
            {
                return TriExtensionInitializationResult.Skip;
            }

            return base.Initialize(definition);
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            if (property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            if (property.FieldType == typeof(int))
            {
                var intField = new IntegerField
                {
                    isDelayed = true,
                };
                intField.BindTri<int, int>(property, v => v, v => Mathf.Max(v, (int) Attribute.min));
                intField.RegisterValueChangedCallback(evt =>
                    intField.SetValueWithoutNotify(Mathf.Max(evt.newValue, (int) Attribute.min)));
                return intField;
            }

            var floatField = new FloatField
            {
                isDelayed = true,
            };
            floatField.BindTri<float, float>(property, v => v, v => Mathf.Max(v, Attribute.min));
            floatField.RegisterValueChangedCallback(evt =>
                floatField.SetValueWithoutNotify(Mathf.Max(evt.newValue, Attribute.min)));
            return floatField;
        }
    }
}
