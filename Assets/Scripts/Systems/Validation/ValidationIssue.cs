using UnityEngine;

namespace Verdict.Systems.Validation
{
    public sealed class ValidationIssue
    {
        public ValidationSeverity Severity { get; }

        public ValidationScope Scope { get; }

        public string Message { get; }

        public string Path { get; }

        public string ContextId { get; }

        public ValidationIssue(
            ValidationSeverity severity,
            ValidationScope scope,
            string message,
            string contextId = null,
            string path = null)
        {
            Severity = severity;
            Scope = scope;
            Message = message;
            ContextId = contextId;
            Path = path;
        }
    }
}
