using UnityEngine;

namespace Verdict.Systems.Validation
{
    public sealed class ValidationIssue
    {
        public ValidationSeverity Severity { get; }

        public ValidationScope Scope { get; }

        public string Message { get; }

        public Object Source { get; }

        public string Path { get; }

        public ValidationIssue(
            ValidationSeverity severity,
            ValidationScope scope,
            string message,
            Object source = null)
        {
            Severity = severity;
            Scope = scope;
            Message = message;
            Source = source;
        }
    }
}
