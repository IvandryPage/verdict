using System;
using UnityEngine;
using Verdict.Common.Comparisons;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Passes when a CourtState stat compares against a threshold the
    /// authored way (e.g. JudgeTrust >= 50).
    /// </summary>
    [Serializable]
    public sealed class CourtStateConditionData : ArgumentConditionData
    {
        [SerializeField]
        private CourtStat stat;

        [SerializeField]
        private ComparisonOperator comparisonOperator;

        [SerializeField]
        private int value;

        public CourtStateConditionData()
        {
        }

        public CourtStateConditionData(CourtStat stat, ComparisonOperator comparisonOperator, int value)
        {
            this.stat = stat;
            this.comparisonOperator = comparisonOperator;
            this.value = value;
        }

        public CourtStat Stat => stat;

        public ComparisonOperator Operator => comparisonOperator;

        public int Value => value;

        public void SetStat(CourtStat newStat) => stat = newStat;

        public void SetOperator(ComparisonOperator op) => comparisonOperator = op;

        public void SetValue(int newValue) => value = newValue;
    }
}
