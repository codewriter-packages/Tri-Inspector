using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(GUIColorDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class GUIColorDrawer : TriAttributeDrawer<GUIColorAttribute>
    {
        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var oldColor = GUI.color;
            var newColor = Color.white;
            
            if (string.IsNullOrEmpty(Attribute.GetColor))
            {
                newColor = Attribute.Color;
            }
            else
            {
                var colorResolver = ValueResolver.Resolve<Color>(property.Definition, Attribute.GetColor ?? "");
                
                newColor = colorResolver.GetValue(property, Color.white);
            }
            
            GUI.color = newColor;
            GUI.contentColor = newColor;
            
            next.OnGUI(position);
            
            GUI.color = oldColor;
            GUI.contentColor = oldColor;
        }
    }
}