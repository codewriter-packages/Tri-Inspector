using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using TriInspector.Utilities;
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
        private const int PaddingRight = 5;

        private ValueResolver<string> _unitResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _unitResolver = ValueResolver.ResolveString(propertyDefinition, Attribute.unitToDisplay);

            if (_unitResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            var unit = _unitResolver.GetValue(property, "");
            var size = Styles.UnitStyle.CalcSize(TriGuiHelper.TempContent(unit));

            var unitRect = new Rect(position.xMax - size.x - PaddingRight, position.y, size.x, position.height);

            // Render the editable input field
            next.OnGUI(position);

            //Change color to grey
            using (TriGuiHelper.PushColor(Color.grey))
            {
                // Render the unit as a suffix in the unitRect
                EditorGUI.LabelField(unitRect, unit);
            }
        }

        private static class Styles
        {
            public static readonly GUIStyle UnitStyle;

            static Styles()
            {
                UnitStyle = new GUIStyle(EditorStyles.label);
            }
        }
    }
}