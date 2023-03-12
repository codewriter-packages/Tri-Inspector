using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(DisplayAsStringDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class DisplayAsStringDrawer : TriAttributeDrawer<DisplayAsStringAttribute>
    {
        public override float GetHeight(float width, TriProperty property, TriElement next)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var value = property.Value;
            var text = value != null ? value.ToString() : "Null";

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            position = EditorGUI.PrefixLabel(position, controlId, property.DisplayNameContent);
            GUI.Label(position, text);
        }
    }
}