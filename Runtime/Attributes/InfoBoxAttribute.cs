using System;
using System.Diagnostics;

namespace TriInspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class InfoBoxAttribute : Attribute
    {
        public string Text { get; }
        public TriMessageType MessageType { get; }

        public InfoBoxAttribute(string text, TriMessageType messageType = TriMessageType.Info)
        {
            Text = text;
            MessageType = messageType;
        }
    }
}