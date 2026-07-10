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
            Object source = null)
        {
            if (condition)
            {
                return;
            }

            result.AddError(
                scope,
                message,
                source);
        }

        public static void Warning(
            ValidationResult result,
            bool condition,
            ValidationScope scope,
            string message,
            Object source = null)
        {
            if (condition)
            {
                return;
            }

            result.AddWarning(
                scope,
                message,
                source);
        }

        public static void Info(
            ValidationResult result,
            ValidationScope scope,
            string message,
            Object source = null)
        {
            result.AddInfo(
                scope,
                message,
                source);
        }
    }
}
