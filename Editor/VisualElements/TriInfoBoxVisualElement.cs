using System;
using TriInspectorUnityInternalBridge;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriInfoBoxVisualElement : VisualElement
    {
        public TriInfoBoxVisualElement(string message, TriMessageType type, Action fixAction = null,
            string fixActionText = null)
        {
            AddToClassList(TriStyles.InfoBox);
            AddToClassList(GetTypeClass(type));

            var icon = EditorGUIUtilityProxy.GetHelpIcon(GetMessageType(type));
            if (icon != null)
            {
                var image = new Image
                {
                    image = icon,
                    scaleMode = ScaleMode.ScaleToFit,
                };
                image.AddToClassList(TriStyles.InfoBoxIcon);
                Add(image);
            }

            var label = new Label(message);
            label.AddToClassList(TriStyles.InfoBoxLabel);
            Add(label);

            if (fixAction != null)
            {
                var button = new Button(fixAction) {text = fixActionText};
                button.AddToClassList(TriStyles.InfoBoxAction);
                Add(button);
            }
        }

        private static string GetTypeClass(TriMessageType type)
        {
            switch (type)
            {
                case TriMessageType.Error: return TriStyles.InfoBoxError;
                case TriMessageType.Warning: return TriStyles.InfoBoxWarning;
                default: return TriStyles.InfoBoxInfo;
            }
        }

        private static MessageType GetMessageType(TriMessageType type)
        {
            switch (type)
            {
                case TriMessageType.Info: return MessageType.Info;
                case TriMessageType.Warning: return MessageType.Warning;
                case TriMessageType.Error: return MessageType.Error;
                default: return MessageType.None;
            }
        }
    }
}