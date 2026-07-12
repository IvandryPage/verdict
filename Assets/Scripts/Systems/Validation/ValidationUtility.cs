using UnityEngine;

namespace Verdict.Systems.Validation
{
    public static class ValidationUtility
    {
        public static void Ensure(
            ValidationResult result,
            bool condition,
            ValidationScope scope,
            string message,
            string contextId = null,
            string path = null)
        {
            if (condition)
            {
                return;
            }

            result.AddError(
                scope,
                message,
                contextId,
                path);
        }

        public static void Warning(
            ValidationResult result,
            bool condition,
            ValidationScope scope,
            string message,
            string contextId = null,
            string path = null)
        {
            if (condition)
            {
                return;
            }

            result.AddWarning(
                scope,
                message,
                contextId,
                path);
        }

        public static void Info(
            ValidationResult result,
            ValidationScope scope,
            string message,
            string contextId = null,
            string path = null)
        {
            result.AddInfo(
                scope,
                message,
                contextId,
                path);
        }
    }
}
