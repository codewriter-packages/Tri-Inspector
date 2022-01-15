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
            using (TriGuiHelper.PushColor(_color))
            {
                GUI.Label(position, string.Empty, Styles.InfoBoxBg);
            }

            GUI.Label(position, _message, Styles.InfoBoxContent);
        }

        private static Color GetColor(MessageType type)
        {
            switch (type)
            {
                case MessageType.Error:
                    return new Color(1f, 0.4f, 0.4f);

                case MessageType.Warning:
                    return new Color(1f, 0.8f, 0.2f);

                default:
                    return Color.white;
            }
        }

        private static class Styles
        {
            public static readonly GUIStyle InfoBoxBg;
            public static readonly GUIStyle InfoBoxContent;

            static Styles()
            {
                InfoBoxBg = new GUIStyle(EditorStyles.helpBox);
                InfoBoxContent = new GUIStyle(EditorStyles.label)
                {
                    padding = InfoBoxBg.padding,
                    fontSize = InfoBoxBg.fontSize,
                    alignment = InfoBoxBg.alignment,
                };
            }
        }
    }
}