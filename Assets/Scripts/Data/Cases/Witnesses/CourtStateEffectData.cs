using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public sealed class CourtStateEffectData
    {
        [SerializeField] private CourtStateEffect effect;

        [Tooltip("Optional target identifier (Statement ID, Testimony ID, Witness ID, Character ID, Ending ID, etc.).")]
        [SerializeField] private string targetId;

        [SerializeField] private CourtStat courtStat;

        [SerializeField] private StatOperation operation;

        [SerializeField] private CharacterStat characterStat;

        [SerializeField]
        private int value;

        public CourtStateEffect Effect => effect;

        public string TargetId => targetId;

        public CourtStat CourtStat => courtStat;

        public StatOperation Operation => operation;

        public CharacterStat CharacterStat => characterStat;

        public int Value => value;
    }
}
