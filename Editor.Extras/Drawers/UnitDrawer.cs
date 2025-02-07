using TriInspector;
using TriInspector.Drawers;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(UnitDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class UnitDrawer : TriAttributeDrawer<UnitAttribute>
    {
        /// <summary>
        /// Defines the padding to the right of the unit label towards the editable input field
        /// </summary>
        private const int paddingRight = 5;
        private GUIStyle unitStyle;
        private GUIContent content;

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            if(unitStyle == null){
                unitStyle = new(EditorStyles.label);
            }

            if(content == null){
                content = new(Attribute.unitToDisplay);
            }

            Vector2 size = unitStyle.CalcSize(content);

            var unitRect = new Rect(position.xMax - size.x - paddingRight, position.y, size.x, position.height);

            // Render the editable input field
            next.OnGUI(position);

            //Change color to grey
            var tmpColor = GUI.color;
            GUI.color = Color.grey;
            // Render the unit as a suffix in the unitRect
            EditorGUI.LabelField(unitRect, content);
            // Restore the original color
            GUI.color = tmpColor;
        }
    }
}