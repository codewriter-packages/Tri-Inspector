using TriInspector.Utilities;
using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriInfoBoxElement : TriElement
    {
        private readonly GUIContent _message;
        private readonly Color _color;

        public TriInfoBoxElement(string message, TriMessageType type = TriMessageType.None, Color? color = null)
        {
            var messageType = GetMessageType(type);
            _message = new GUIContent(message, EditorGUIUtilityProxy.GetHelpIcon(messageType));
            _color = color ?? GetColor(type);
        }

        public override float GetHeight(float width)
        {
            return EditorStyles.helpBox.CalcHeight(_message, width);
        }

        public override void OnGUI(Rect position)
        {
            position = EditorGUI.IndentedRect(position);

            using (TriGuiHelper.PushColor(_color))
            {
                GUI.Label(position, string.Empty, Styles.InfoBoxBg);
            }

            GUI.Label(position, _message, Styles.InfoBoxContent);
        }

        private static Color GetColor(TriMessageType type)
        {
            switch (type)
            {
                case TriMessageType.Error:
                    return new Color(1f, 0.4f, 0.4f);

                case TriMessageType.Warning:
                    return new Color(1f, 0.8f, 0.2f);

                default:
                    return Color.white;
            }
        }

        private static MessageType GetMessageType(TriMessageType type)
        {
            switch (type)
            {
                case TriMessageType.None: return MessageType.None;
                case TriMessageType.Info: return MessageType.Info;
                case TriMessageType.Warning: return MessageType.Warning;
                case TriMessageType.Error: return MessageType.Error;
                default: return MessageType.None;
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
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true,
                };
            }
        }
    }
}