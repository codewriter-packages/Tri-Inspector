using System;
using TriInspector.Utilities;
using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    public class TriInfoBoxElement : TriElement
    {
        private const int ActionSpacing = 5;
        private const int ActionWidth = 100;
        private const int ActionWidthWithSpacing = ActionWidth + ActionSpacing * 2;

        private readonly GUIContent _message;
        private readonly Texture2D _icon;
        private readonly Color _color;
        private readonly Action _inlineAction;
        private readonly GUIContent _inlineActionContent;

        public TriInfoBoxElement(string message, TriMessageType type = TriMessageType.None, Color? color = null,
            Action inlineAction = null, GUIContent inlineActionContent = null)
        {
            var messageType = GetMessageType(type);
            _icon = EditorGUIUtilityProxy.GetHelpIcon(messageType);
            _message = new GUIContent(message);
            _color = color ?? GetColor(type);
            _inlineAction = inlineAction;
            _inlineActionContent = inlineActionContent ?? GUIContent.none;
        }

        public override float GetHeight(float width)
        {
            var labelWidth = width;

            if (_inlineAction != null)
            {
                labelWidth -= ActionWidthWithSpacing;
            }

            var style = _icon == null ? Styles.InfoBoxContentNone : Styles.InfoBoxContent;
            var height = style.CalcHeight(_message, labelWidth);

            if (_inlineAction != null)
            {
                height = Mathf.Max(height, CalcActionHeight() + ActionSpacing * 2);
            }

            return Mathf.Max(26, height);
        }

        public override void OnGUI(Rect position)
        {
            using (TriGuiHelper.PushColor(_color))
            {
                GUI.Label(position, string.Empty, Styles.InfoBoxBg);
            }

            var labelWidth = position.width;

            if (_inlineAction != null)
            {
                labelWidth -= ActionWidthWithSpacing;
            }

            if (_icon != null)
            {
                var labelRect = new Rect(position)
                {
                    width = labelWidth,
                };

                var iconRect = new Rect(position)
                {
                    xMin = position.xMin + 4,
                    width = 20,
                };

                GUI.Label(labelRect, _message, Styles.InfoBoxContent);
                GUI.DrawTexture(iconRect, _icon, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Label(position, _message, Styles.InfoBoxContentNone);
            }

            if (_inlineAction != null)
            {
                var fixHeight = CalcActionHeight();

                var actionRect = new Rect(position)
                {
                    xMax = position.xMax - ActionSpacing,
                    xMin = position.xMax - ActionWidth - ActionSpacing,
                    yMin = position.center.y - fixHeight / 2,
                    yMax = position.center.y + fixHeight / 2,
                };

                if (GUI.Button(actionRect, _inlineActionContent, Styles.InfoBoxInlineAction))
                {
                    _inlineAction?.Invoke();
                }
            }
        }


        private float CalcActionHeight()
        {
            return Styles.InfoBoxInlineAction.CalcHeight(_inlineActionContent, ActionWidth);
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
            public static readonly GUIStyle InfoBoxContentNone;
            public static readonly GUIStyle InfoBoxInlineAction;

            static Styles()
            {
                InfoBoxBg = new GUIStyle(EditorStyles.helpBox);
                InfoBoxContentNone = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(4, 4, 4, 4),
                    fontSize = InfoBoxBg.fontSize,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true,
                };
                InfoBoxContent = new GUIStyle(InfoBoxContentNone)
                {
                    padding = new RectOffset(26, 4, 4, 4),
                };
                InfoBoxInlineAction = new GUIStyle(GUI.skin.button)
                {
                    wordWrap = true,
                };
            }
        }
    }
}