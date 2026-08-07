using System;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Data.Cases
{
    [Serializable]
    public sealed class CourtStateEffectData
    {
        [SerializeField] private CourtStateEffect effect;

        [SerializeField] private EffectTargetType targetType;

        [Tooltip("Optional target identifier (Statement ID, Testimony ID, Witness ID, Character ID, Evidence ID, etc.).")]
        [SerializeField] private string targetId;

        [SerializeField] private CourtStat courtStat;

        [SerializeField] private StatOperation operation;

        [SerializeField] private CharacterStat characterStat;

        [SerializeField]
        private int value;

        public CourtStateEffect Effect => effect;

        public EffectTargetType TargetType => targetType;

        public string TargetId => targetId;

        public CourtStat CourtStat => courtStat;

        public StatOperation Operation => operation;

        public CharacterStat CharacterStat => characterStat;

        public int Value => value;

        public bool RequiresTarget =>
            TargetType != EffectTargetType.None;

        public bool HasTarget =>
            !string.IsNullOrWhiteSpace(TargetId);

        public void SetEffect(CourtStateEffect effect)
        {
            this.effect = effect;

            switch (effect)
            {
                case CourtStateEffect.ModifyCharacterStat:
                    SetTargetType(EffectTargetType.Character);
                    break;

                case CourtStateEffect.UnlockEvidence:
                    SetTargetType(EffectTargetType.Evidence);
                    break;

                case CourtStateEffect.RevealStatement:
                case CourtStateEffect.JumpStatement:
                    SetTargetType(EffectTargetType.Statement);
                    break;

                case CourtStateEffect.RevealWitness:
                case CourtStateEffect.JumpWitness:
                    SetTargetType(EffectTargetType.Witness);
                    break;

                case CourtStateEffect.RevealTestimony:
                case CourtStateEffect.JumpTestimony:
                    SetTargetType(EffectTargetType.Testimony);
                    break;

                default:
                    SetTargetType(EffectTargetType.None);
                    break;
            }
        }

        public void SetTargetType(EffectTargetType newTargetType) => targetType = newTargetType;

        public void SetTargetId(string newTargetId) => targetId = newTargetId;

        public void SetCourtStat(CourtStat newStat) => courtStat = newStat;

        public void SetOperation(StatOperation newOperation) => operation = newOperation;

        public void SetCharacterStat(CharacterStat newStat) => characterStat = newStat;

        public void SetValue(int newValue) => value = newValue;
    }
}
