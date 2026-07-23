using System;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Data.Narrative
{
    public enum NarrativeConditionMode
    {
        /// <summary>Compare a CourtState stat against a threshold.</summary>
        CourtStat,

        /// <summary>Check whether a specific Claim has been resolved.</summary>
        ClaimResolved
    }

    /// <summary>
    /// A condition Narrative can react to. Narrative never asks "which
    /// evidence?" - it only ever asks "has condition X been met?": either
    /// a CourtState threshold, or a specific Claim's resolution state
    /// (e.g. "Claim A resolved -&gt; new dialogue branch"). Referencing a
    /// Claim by id keeps this at arm's length from gameplay internals -
    /// same pattern as StatementNodeData.StatementId.
    /// </summary>
    [Serializable]
    public sealed class NarrativeConditionData
    {
        [SerializeField]
        private NarrativeConditionMode mode = NarrativeConditionMode.CourtStat;

        [SerializeField]
        private CourtStat stat;

        [SerializeField]
        private NarrativeComparisonOperator comparisonOperator;

        [SerializeField]
        private int value;

        [SerializeField]
        private string claimId;

        [SerializeField]
        private bool requireSuccessful = true;

        public NarrativeConditionMode Mode => mode;

        public CourtStat Stat => stat;

        public NarrativeComparisonOperator Operator => comparisonOperator;

        public int Value => value;

        public string ClaimId => claimId;

        /// <summary>
        /// When Mode is ClaimResolved: if true, the claim must have
        /// resolved successfully; if false, any resolution (including a
        /// failed attempt) satisfies the condition.
        /// </summary>
        public bool RequireSuccessful => requireSuccessful;

        public void SetMode(NarrativeConditionMode newMode)
        {
            mode = newMode;
        }

        public void SetStat(CourtStat newStat)
        {
            stat = newStat;
        }

        public void SetOperator(NarrativeComparisonOperator newOperator)
        {
            comparisonOperator = newOperator;
        }

        public void SetValue(int newValue)
        {
            value = newValue;
        }

        public void SetClaimId(string id)
        {
            claimId = id;
        }

        public void SetRequireSuccessful(bool value)
        {
            requireSuccessful = value;
        }

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
