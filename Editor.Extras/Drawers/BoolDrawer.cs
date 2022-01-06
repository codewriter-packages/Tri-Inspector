using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(BoolDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class BoolDrawer : TriValueDrawer<bool>
    {
        public override TriElement CreateElement(TriValue<bool> propertyValue, TriElement next)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return base.CreateElement(propertyValue, next);
        }

        public override float GetHeight(float width, TriValue<bool> propertyValue, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, TriValue<bool> propertyValue, TriElement next)
        {
            var value = propertyValue.Value;

            EditorGUI.BeginChangeCheck();

            value = EditorGUI.Toggle(position, propertyValue.Property.DisplayNameContent, value);

            if (EditorGUI.EndChangeCheck())
            {
                propertyValue.Value = value;
            }
        }
    }
}