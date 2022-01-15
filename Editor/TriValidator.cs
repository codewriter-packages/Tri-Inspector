using System;
using JetBrains.Annotations;
using UnityEditor;

namespace TriInspector
{
    public abstract class TriValidator
    {
        internal bool ApplyOnArrayElement { get; set; }

        [PublicAPI]
        public abstract TriValidationResult Validate(TriProperty property);
    }

    public abstract class TriAttributeValidator : TriValidator
    {
        internal Attribute RawAttribute { get; set; }
    }

    public abstract class TriAttributeValidator<TAttribute> : TriAttributeValidator
        where TAttribute : Attribute
    {
        [PublicAPI]
        public TAttribute Attribute => (TAttribute) RawAttribute;
    }

    public abstract class TriValueValidator : TriValidator
    {
    }

    public abstract class TriValueValidator<T> : TriValueValidator
    {
        public sealed override TriValidationResult Validate(TriProperty property)
        {
            return Validate(new TriValue<T>(property));
        }

        [PublicAPI]
        public abstract TriValidationResult Validate(TriValue<T> propertyValue);
    }

    public readonly struct TriValidationResult
    {
        public static TriValidationResult Valid => new TriValidationResult(null, MessageType.None);

        private TriValidationResult(string message, MessageType messageType)
        {
            Message = message;
            MessageType = messageType;
        }

        public string Message { get; }
        public MessageType MessageType { get; }

        public static TriValidationResult Error(string error)
        {
            return new TriValidationResult(error, MessageType.Error);
        }

        public static TriValidationResult Warning(string error)
        {
            return new TriValidationResult(error, MessageType.Warning);
        }
    }
}