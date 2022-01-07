using UnityEditor;
using UnityEngine;

namespace TriInspector.Drawers
{
    public abstract class BuiltinDrawerBase<T> : TriValueDrawer<T>
    {
        public sealed override TriElement CreateElement(TriValue<T> propertyValue, TriElement next)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return base.CreateElement(propertyValue, next);
        }

        public sealed override float GetHeight(float width, TriValue<T> propertyValue, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public sealed override void OnGUI(Rect position, TriValue<T> propertyValue, TriElement next)
        {
            var value = propertyValue.Value;

            EditorGUI.BeginChangeCheck();

            value = OnValueGUI(position, propertyValue.Property.DisplayNameContent, value);

            if (EditorGUI.EndChangeCheck())
            {
                propertyValue.Value = value;
            }
        }

        protected abstract T OnValueGUI(Rect position, GUIContent label, T value);
    }
}