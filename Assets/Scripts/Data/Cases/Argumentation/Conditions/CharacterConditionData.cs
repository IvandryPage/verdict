using System;
using UnityEngine;
using Verdict.Common.Comparisons;
using Verdict.Data.Characters;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Passes when a specific character's stat compares against a
    /// threshold (e.g. this witness's Stress &gt;= 70).
    /// </summary>
    [Serializable]
    public sealed class CharacterConditionData : ArgumentConditionData
    {
        [SerializeField]
        private CharacterData character;

        [SerializeField]
        private CharacterStat stat;

        [SerializeField]
        private ComparisonOperator comparisonOperator;

        [SerializeField]
        private int value;

        public CharacterConditionData()
        {
        }

        public CharacterConditionData(CharacterData character, CharacterStat stat, ComparisonOperator comparisonOperator, int value)
        {
            this.character = character;
            this.stat = stat;
            this.comparisonOperator = comparisonOperator;
            this.value = value;
        }

        public CharacterData Character => character;

        public CharacterStat Stat => stat;

        public ComparisonOperator Operator => comparisonOperator;

        public int Value => value;

        public void SetCharacter(CharacterData newCharacter) => character = newCharacter;

        public void SetStat(CharacterStat newStat) => stat = newStat;

        public void SetOperator(ComparisonOperator op) => comparisonOperator = op;

        public void SetValue(int newValue) => value = newValue;
    }
}
