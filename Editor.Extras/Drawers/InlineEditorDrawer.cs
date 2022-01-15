using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: RegisterTriAttributeDrawer(typeof(InlineEditorDrawer), TriDrawerOrder.Drawer - 100,
    ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class InlineEditorDrawer : TriAttributeDrawer<InlineEditorAttribute>
    {
        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            if (!typeof(Object).IsAssignableFrom(property.FieldType))
            {
                var stack = new TriElement();
                stack.AddChild(new TriInfoBoxElement($"InlineEditor valid only on Object fields",
                    MessageType.Error));
                stack.AddChild(next);

                return stack;
            }

            var element = new TriBoxGroupElement(new DeclareBoxGroupAttribute(""));
            element.AddChild(new ObjectReferenceFoldoutDrawerElement(property));
            element.AddChild(new InlineEditorElement(property));
            return element;
        }

        private class ObjectReferenceFoldoutDrawerElement : TriElement
        {
            private readonly TriProperty _property;

            public ObjectReferenceFoldoutDrawerElement(TriProperty property)
            {
                _property = property;
            }

            public override float GetHeight(float width)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position)
            {
                var prefixRect = new Rect(position)
                {
                    height = EditorGUIUtility.singleLineHeight,
                    xMax = position.xMin + EditorGUIUtility.labelWidth,
                };
                var pickerRect = new Rect(position)
                {
                    height = EditorGUIUtility.singleLineHeight,
                    xMin = prefixRect.xMax,
                };

                using (TriGuiHelper.PushIndentLevel())
                {
                    TriEditorGUI.Foldout(prefixRect, _property);
                }

                EditorGUI.BeginChangeCheck();

                var allowSceneObjects = _property.PropertyTree.TargetObjects[0] is var targetObject &&
                                        targetObject != null && !EditorUtility.IsPersistent(targetObject);

                var value = (Object) _property.Value;
                value = EditorGUI.ObjectField(pickerRect, GUIContent.none, value,
                    _property.FieldType, allowSceneObjects);

                if (EditorGUI.EndChangeCheck())
                {
                    _property.SetValue(value);
                }
            }
        }
    }
}