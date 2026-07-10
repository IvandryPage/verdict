using System;
using System.Linq;
using System.Text;

namespace Verdict.Systems.Validation
{
    /// <summary>
    /// Thrown when one or more validation errors prevent a case from being loaded.
    /// </summary>
    public sealed class ValidationException : Exception
    {
        public ValidationResult Result { get; }

        public string FormattedMessage => CreateMessage(Result);

        public ValidationException(ValidationResult result)
            : base(CreateMessage(result))
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public ValidationException(
            ValidationResult result,
            Exception innerException)
            : base(CreateMessage(result), innerException)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        private static string CreateMessage(
            ValidationResult result)
        {
            if (result == null)
            {
                return "Validation failed.";
            }

            var errors = result.Issues
                .Where(issue => issue.Severity == ValidationSeverity.Error)
                .ToList();

            if (errors.Count == 0)
            {
                return "Validation completed without errors.";
            }

            StringBuilder builder = new();

            builder.AppendLine($"Validation failed with {errors.Count} error(s).");
            builder.AppendLine();

            foreach (ValidationIssue issue in errors)
            {
                builder.Append("• ");

                builder.Append('[');
                builder.Append(issue.Scope);
                builder.Append("] ");

                builder.AppendLine(issue.Message);
            }

            return builder.ToString();
        }
    }
}
