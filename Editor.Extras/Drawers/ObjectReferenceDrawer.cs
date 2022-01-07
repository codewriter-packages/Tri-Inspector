using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(ObjectReferenceDrawer<>), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class ObjectReferenceDrawer<T> : TriValueDrawer<T>
        where T : Object
    {
        public override TriElement CreateElement(TriValue<T> value, TriElement next)
        {
            if (value.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return new ObjectReferenceDrawerElement(value);
        }

        private class ObjectReferenceDrawerElement : TriElement
        {
            private readonly TriValue<T> _propertyValue;

            public ObjectReferenceDrawerElement(TriValue<T> propertyValue)
            {
                _propertyValue = propertyValue;
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                var value = _propertyValue.Value;

                EditorGUI.BeginChangeCheck();

                var allowSceneObjects = _propertyValue.Property.PropertyTree.TargetObjects[0] is var targetObject &&
                                        targetObject != null && !EditorUtility.IsPersistent(targetObject);

                value = (T) EditorGUI.ObjectField(position, _propertyValue.Property.DisplayNameContent, value,
                    _propertyValue.Property.FieldType, allowSceneObjects);

                if (EditorGUI.EndChangeCheck())
                {
                    _propertyValue.Value = value;
                }
            }
        }
    }
}