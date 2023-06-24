using System;
using UnityEngine;

namespace TriInspector
{
    public readonly struct TriValidationResult
    {
        public static TriValidationResult Valid => new TriValidationResult(true, null, TriMessageType.None);

        public TriValidationResult(bool valid, string message, TriMessageType messageType,
            Action fixAction = null, GUIContent fixActionContent = null)
        {
            IsValid = valid;
            Message = message;
            MessageType = messageType;
            FixAction = fixAction;
            FixActionContent = fixActionContent;
        }

        public bool IsValid { get; }
        public string Message { get; }
        public TriMessageType MessageType { get; }
        public Action FixAction { get; }
        public GUIContent FixActionContent { get; }

        public TriValidationResult WithFix(Action action, string name = null)
        {
            return new TriValidationResult(IsValid, Message, MessageType, action, new GUIContent(name ?? "Fix"));
        }

        public static TriValidationResult Info(string error)
        {
            return new TriValidationResult(false, error, TriMessageType.Info);
        }

        public static TriValidationResult Error(string error)
        {
            return new TriValidationResult(false, error, TriMessageType.Error);
        }

        public static TriValidationResult Warning(string error)
        {
            return new TriValidationResult(false, error, TriMessageType.Warning);
        }
    }

    public enum TriMessageType
    {
        None,
        Info,
        Warning,
        Error,
    }
}