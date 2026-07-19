using System;

namespace Verdict.Common.Comparisons
{
    public enum ComparisonOperator
    {
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual,
        Equal,
        NotEqual
    }

    /// <summary>
    /// Pure, stateless comparison evaluation. Shared by any system that
    /// needs to compare a runtime value against an authored threshold
    /// (ArgumentConditions, future rule systems, etc).
    /// </summary>
    public static class Comparison
    {
        public static bool Evaluate(int current, ComparisonOperator op, int target)
        {
            return op switch
            {
                ComparisonOperator.GreaterThan => current > target,
                ComparisonOperator.GreaterOrEqual => current >= target,
                ComparisonOperator.LessThan => current < target,
                ComparisonOperator.LessOrEqual => current <= target,
                ComparisonOperator.Equal => current == target,
                ComparisonOperator.NotEqual => current != target,
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
        }

        public static bool Evaluate(float current, ComparisonOperator op, float target)
        {
            return op switch
            {
                ComparisonOperator.GreaterThan => current > target,
                ComparisonOperator.GreaterOrEqual => current >= target,
                ComparisonOperator.LessThan => current < target,
                ComparisonOperator.LessOrEqual => current <= target,
                ComparisonOperator.Equal => Math.Abs(current - target) < 0.0001f,
                ComparisonOperator.NotEqual => Math.Abs(current - target) >= 0.0001f,
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
        }
    }
}
