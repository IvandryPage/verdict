using System.Linq;
using Verdict.Editor.CaseEditor.Theme;
using Verdict.Systems.Validation;

namespace Verdict.Editor.CaseEditor.Validation
{
    public static class ValidationOverlayBuilder
    {
        public static NodeColor GetNodeColor(
            ValidationResult result,
            string statementId)
        {
            if (result == null)
            {
                return NodeColor.Default;
            }

            if (result.GetIssues(
                    ValidationScope.Statement,
                    statementId,
                    ValidationSeverity.Error)
                .Any())
            {
                return NodeColor.Error;
            }

            if (result.GetIssues(
                    ValidationScope.Statement,
                    statementId,
                    ValidationSeverity.Warning)
                .Any())
            {
                return NodeColor.Warning;
            }

            return NodeColor.Default;
        }

        public static bool HasError(
            ValidationResult result,
            string statementId)
        {
            return result != null &&
                   result.GetIssues(
                       ValidationScope.Statement,
                       statementId,
                       ValidationSeverity.Error)
                   .Any();
        }

        public static bool HasWarning(
            ValidationResult result,
            string statementId)
        {
            return result != null &&
                   result.GetIssues(
                       ValidationScope.Statement,
                       statementId,
                       ValidationSeverity.Warning)
                   .Any();
        }
    }
}
