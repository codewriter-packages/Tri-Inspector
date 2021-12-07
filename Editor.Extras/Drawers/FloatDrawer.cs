using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriDrawer(typeof(FloatDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class FloatDrawer : TriValueDrawer<float>
    {
        public override TriElement CreateElement(TriValue<float> propertyValue, TriElement next)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return base.CreateElement(propertyValue, next);
        }

        public override float GetHeight(float width, TriValue<float> propertyValue, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, TriValue<float> propertyValue, TriElement next)
        {
            var value = propertyValue.Value;

            EditorGUI.BeginChangeCheck();

            value = EditorGUI.FloatField(position, propertyValue.Property.DisplayNameContent, value);

            if (EditorGUI.EndChangeCheck())
            {
                propertyValue.Value = value;
            }
        }
    }
}