using System;

namespace TriInspector.Validator
{
    internal readonly struct TriValidationIssueKey : IEquatable<TriValidationIssueKey>
    {
        public readonly TriMessageType severity;
        public readonly string message;
        public readonly string propertyPath;

        public TriValidationIssueKey(TriMessageType severity, string message, string propertyPath)
        {
            this.severity = severity;
            this.message = message;
            this.propertyPath = propertyPath;
        }

        public static TriValidationIssueKey From(TriValidationIssue issue)
        {
            return new TriValidationIssueKey(
                issue.result.MessageType, issue.result.Message, issue.property?.PropertyPath);
        }

        public static TriValidationIssueKey From(TriValidationNodeData data)
        {
            return new TriValidationIssueKey(data.severity, data.label, data.propertyPath);
        }

        public bool Equals(TriValidationIssueKey other)
        {
            return severity == other.severity &&
                   message == other.message &&
                   propertyPath == other.propertyPath;
        }

        public override bool Equals(object obj)
        {
            return obj is TriValidationIssueKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int) severity;
                hash = (hash * 397) ^ (message != null ? message.GetHashCode() : 0);
                hash = (hash * 397) ^ (propertyPath != null ? propertyPath.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
