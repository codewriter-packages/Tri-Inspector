using System.Reflection;
using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(ButtonDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class ButtonDrawer : TriAttributeDrawer<ButtonAttribute>
    {
        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var name = Attribute.Name ?? property.DisplayName;

            if (GUI.Button(position, name))
            {
                var methodInfo = (MethodInfo) property.MemberInfo;

                for (var i = 0; i < property.PropertyTree.TargetObjects.Length; i++)
                {
                    var parentValue = property.Parent.GetValue(i);
                    methodInfo.Invoke(parentValue, new object[0]);
                }
            }
        }
    }
}