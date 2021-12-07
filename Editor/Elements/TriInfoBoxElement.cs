using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriInfoBoxElement : TriElement
    {
        private readonly GUIContent _message;
        private readonly Color _color;

        public TriInfoBoxElement(string message, MessageType type = MessageType.None, Color? color = null)
        {
            _message = new GUIContent(message, EditorGUIUtilityProxy.GetHelpIcon(type));
            _color = color ?? GetColor(type);
        }

        public override float GetHeight(float width)
        {
            return EditorStyles.helpBox.CalcHeight(_message, width);
        }

        public override void OnGUI(Rect position)
        {
            TriGuiHelper.PushColor(_color);
            GUI.Label(position, _message, EditorStyles.helpBox);
            TriGuiHelper.PopColor();
        }

        private static Color GetColor(MessageType type)
        {
            switch (type)
            {
                case MessageType.Error:
                    return Color.red;

                case MessageType.Warning:
                    return Color.yellow;

                default:
                    return Color.white;
            }
        }
    }
}