using System;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// A condition evaluated against CourtStateRuntime at runtime.
    /// Kept minimal on purpose - one stat comparison. Composite/boolean
    /// conditions can be built later as additional condition node chains.
    /// </summary>
    [Serializable]
    public sealed class NarrativeConditionData
    {
        [SerializeField]
        private CourtStat stat;

        [SerializeField]
        private NarrativeComparisonOperator comparisonOperator;

        [SerializeField]
        private int value;

        public CourtStat Stat => stat;

        public NarrativeComparisonOperator Operator => comparisonOperator;

        public int Value => value;

        public bool Evaluate(int currentValue)
        {
            return comparisonOperator switch
            {
                NarrativeComparisonOperator.GreaterThan => currentValue > value,
                NarrativeComparisonOperator.GreaterOrEqual => currentValue >= value,
                NarrativeComparisonOperator.LessThan => currentValue < value,
                NarrativeComparisonOperator.LessOrEqual => currentValue <= value,
                NarrativeComparisonOperator.Equal => currentValue == value,
                NarrativeComparisonOperator.NotEqual => currentValue != value,
                _ => throw new ArgumentOutOfRangeException(nameof(comparisonOperator))
            };
        }
    }
}
