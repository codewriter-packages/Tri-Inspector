using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(LayerDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class LayerDrawer : TriAttributeDrawer<LayerAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var type = propertyDefinition.FieldType;
            if (type != typeof(int))
            {
                return "Layer attribute can only be used on field with int type";
            }

            return base.Initialize(propertyDefinition);
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new LayerElement(property);
        }

        private class LayerElement : TriElement
        {
            private readonly TriProperty _property;

            public LayerElement(TriProperty property)
            {
                _property = property;
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                EditorGUI.BeginChangeCheck();

                var currentValue = (int)_property.Value;
                var newValue = EditorGUI.LayerField(position, _property.DisplayName, currentValue);

                if (EditorGUI.EndChangeCheck())
                {
                    _property.SetValue(newValue);
                }
            }
        }
    }
}

