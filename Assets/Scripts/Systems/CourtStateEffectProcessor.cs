using System;
using Verdict.Data.Cases;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CourtStateEffectProcessor
    {
        private readonly CaseRuntime caseRuntime;
        private readonly CourtStateRuntime courtState;

        public CourtStateEffectProcessor(
            CaseRuntime caseRuntime,
            CourtStateRuntime courtState)
        {
            this.caseRuntime = caseRuntime
                ?? throw new ArgumentNullException(nameof(caseRuntime));

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
                    // Persist revealed statement ID for save/load, but gameplay uses runtime visibility directly.
                    courtState.RevealStatement(effectData.TargetId);
                    TryRevealRuntimeStatement(effectData.TargetId);
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
                    throw new ArgumentOutOfRangeException(nameof(effectData.Effect), effectData.Effect, null);
            }
        }

        private void TryRevealRuntimeStatement(string statementId)
        {
            if (string.IsNullOrWhiteSpace(statementId))
            {
                return;
            }

            foreach (WitnessRuntime witness in caseRuntime.Witnesses)
            {
                foreach (TestimonyRuntime testimony in witness.Testimonies)
                {
                    foreach (StatementRuntime statement in testimony.Statements)
                    {
                        if (statement.Data.Id == statementId)
                        {
                            statement.IsVisible = true;
                            return;
                        }
                    }
                }
            }
        }
    }
}
