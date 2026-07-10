using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verdict.Systems.Validation
{
    public sealed class ValidationResult
    {
        private readonly List<ValidationIssue> _issues = new();

        public IReadOnlyList<ValidationIssue> Issues => _issues;

        public bool IsValid => !HasErrors;

        public bool HasIssues =>
            _issues.Count > 0;

        public bool HasErrors =>
            _issues.Any(issue => issue.Severity == ValidationSeverity.Error);

        public bool HasWarnings =>
            _issues.Any(issue => issue.Severity == ValidationSeverity.Warning);

        public bool HasInfo =>
            _issues.Any(issue => issue.Severity == ValidationSeverity.Info);

        public IEnumerable<ValidationIssue> Errors =>
            _issues.Where(i => i.Severity == ValidationSeverity.Error);

        public IEnumerable<ValidationIssue> Warnings =>
            _issues.Where(i => i.Severity == ValidationSeverity.Warning);

        public IEnumerable<ValidationIssue> Infos =>
            _issues.Where(i => i.Severity == ValidationSeverity.Info);

        public int ErrorCount => Errors.Count();
        public int WarningCount => Warnings.Count();
        public int InfoCount => Infos.Count();

        public ValidationIssue AddIssue(
            ValidationSeverity severity,
            ValidationScope scope,
            string message,
            Object source = null)
        {
            var issue = new ValidationIssue(severity, scope, message, source);

            _issues.Add(issue);

            return issue;
        }

        public void AddError(
            ValidationScope scope,
            string message,
            Object source = null)
        {
            AddIssue(
                ValidationSeverity.Error,
                scope,
                message,
                source);
        }

        public void AddWarning(
            ValidationScope scope,
            string message,
            Object source = null)
        {
            AddIssue(
                ValidationSeverity.Warning,
                scope,
                message,
                source);
        }

        public void AddInfo(
            ValidationScope scope,
            string message,
            Object source = null)
        {
            AddIssue(
                ValidationSeverity.Info,
                scope,
                message,
                source);
        }

        public void Merge(ValidationResult other)
        {
            if (other == null)
            {
                return;
            }

            if (ReferenceEquals(this, other))
            {
                return;
            }

            _issues.AddRange(other.Issues);
        }

        public void Clear()
        {
            _issues.Clear();
        }

        public void ThrowIfInvalid()
        {
            if (HasErrors)
            {
                throw new ValidationException(this);
            }
        }
    }
}
