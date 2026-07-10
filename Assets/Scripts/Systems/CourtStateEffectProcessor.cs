using System;
using Verdict.Data.Cases;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CourtStateEffectProcessor
    {
        private readonly CourtStateRuntime courtState;

        public CourtStateEffectProcessor(CourtStateRuntime courtState)
        {
            this.courtState = courtState
                ?? throw new ArgumentNullException(nameof(courtState));
        }

        public void Apply(EvaluationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!result.HasMatchedRule)
            {
                return;
            }

            var effects = result.IsSuccess
                ? result.MatchedRule.SuccessEffects
                : result.MatchedRule.FailureEffects;

            foreach (CourtStateEffectData effect in effects)
            {
                if (effect == null)
                {
                    continue;
                }
                ApplyEffect(effect);
            }
        }

        private void ApplyEffect(CourtStateEffectData effectData)
        {
            switch (effectData.Effect)
            {
                case CourtStateEffect.None:
                    break;

                case CourtStateEffect.IncreasePenalty:
                    courtState.IncreasePenalty(effectData.Value);
                    break;

                case CourtStateEffect.DecreasePenalty:
                    courtState.DecreasePenalty(effectData.Value);
                    break;

                case CourtStateEffect.IncreaseTrust:
                    courtState.IncreaseTrust(effectData.Value);
                    break;

                case CourtStateEffect.DecreaseTrust:
                    courtState.DecreaseTrust(effectData.Value);
                    break;

                case CourtStateEffect.RevealNewStatement:
                    courtState.RevealStatement(effectData.TargetId);
                    break;

                case CourtStateEffect.RevealNewTestimony:
                    courtState.RevealTestimony(effectData.TargetId);
                    break;

                case CourtStateEffect.UnlockEvidence:
                    courtState.UnlockEvidence(effectData.TargetId);
                    break;

                case CourtStateEffect.TriggerEnding:
                    // Akan ditangani oleh EndingResolver / CourtroomController.
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
