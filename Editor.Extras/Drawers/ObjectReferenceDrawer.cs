using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(ObjectReferenceDrawer), TriDrawerOrder.Fallback)]

namespace TriInspector.Drawers
{
    public class ObjectReferenceDrawer : TriValueDrawer<Object>
    {
        public override TriElement CreateElement(TriValue<Object> value, TriElement next)
        {
            if (value.Property.IsRootProperty)
            {
                return next;
            }

            return new ObjectReferenceDrawerElement(value);
        }

        private class ObjectReferenceDrawerElement : TriElement
        {
            private TriValue<Object> _propertyValue;
            private readonly bool _allowSceneObjects;

            public ObjectReferenceDrawerElement(TriValue<Object> propertyValue)
            {
                _propertyValue = propertyValue;
                _allowSceneObjects = propertyValue.Property.PropertyTree.TargetIsPersistent &&
                                     propertyValue.Property.TryGetAttribute(out AssetsOnlyAttribute _) == false;
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                var hasSerializedProperty = _propertyValue.Property
                    .TryGetSerializedProperty(out var serializedProperty);

                var value = hasSerializedProperty ? serializedProperty.objectReferenceValue : _propertyValue.SmartValue;

                EditorGUI.BeginChangeCheck();

                value = EditorGUI.ObjectField(position, _propertyValue.Property.DisplayNameContent, value,
                    _propertyValue.Property.FieldType, _allowSceneObjects);

                if (EditorGUI.EndChangeCheck())
                {
                    if (hasSerializedProperty)
                    {
                        serializedProperty.objectReferenceValue = value;
                        _propertyValue.Property.NotifyValueChanged();
                    }
                    else
                    {
                        _propertyValue.SmartValue = value;
                    }
                }
            }
        }
    }
}