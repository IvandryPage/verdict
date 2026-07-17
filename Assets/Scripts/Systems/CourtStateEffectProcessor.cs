using System;
using System.Collections.Generic;
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

        public CourtStateEffectProcessingResult Apply(EvaluationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!result.HasMatchedRule)
            {
                return new CourtStateEffectProcessingResult(Array.Empty<CourtStateEffectIntent>());
            }

            var effects = result.IsSuccess
                ? result.MatchedRule.SuccessEffects
                : result.MatchedRule.FailureEffects;

            var intents = new List<CourtStateEffectIntent>();
            foreach (CourtStateEffectData effect in effects)
            {
                if (effect == null)
                {
                    continue;
                }

                CourtStateEffectIntent intent = ApplyEffect(effect);
                if (intent != null)
                {
                    intents.Add(intent);
                }
            }

            return new CourtStateEffectProcessingResult(intents);
        }

        private CourtStateEffectIntent ApplyEffect(CourtStateEffectData effectData)
        {
            switch (effectData.Effect)
            {
                case CourtStateEffect.None:
                    break;

                case CourtStateEffect.RevealStatement:
                    // Persist revealed statement ID for save/load, but gameplay uses runtime visibility directly.
                    courtState.RevealStatement(effectData.TargetId);
                    TryRevealRuntimeStatement(effectData.TargetId);
                    break;

                case CourtStateEffect.RevealTestimony:
                    courtState.RevealTestimony(effectData.TargetId);
                    TryRevealRuntimeTestimony(effectData.TargetId);
                    break;

                case CourtStateEffect.RevealWitness:
                    // Persist revealed witness (as testimony IDs are per-witness) and reveal runtime statements.
                    // We'll reveal all testimonies/statements for the witness at runtime.
                    TryRevealRuntimeWitness(effectData.TargetId);
                    break;

                case CourtStateEffect.UnlockEvidence:
                    courtState.UnlockEvidence(effectData.TargetId);
                    break;

                case CourtStateEffect.ModifyCourtStat:
                    courtState.ModifyCourtStat(
                        effectData.CourtStat,
                        effectData.Value,
                        effectData.Operation);
                    break;

                case CourtStateEffect.ModifyCharacterStat:
                    if (!string.IsNullOrWhiteSpace(effectData.TargetId) &&
                        caseRuntime.TryGetWitness(effectData.TargetId, out WitnessRuntime targetWitness))
                    {
                        targetWitness.ModifyCharacterStat(
                            effectData.CharacterStat,
                            effectData.Value,
                            effectData.Operation);
                    }

                    break;

                case CourtStateEffect.JumpStatement:
                case CourtStateEffect.JumpTestimony:
                case CourtStateEffect.JumpWitness:
                    return new CourtStateEffectIntent(effectData.Effect, effectData.TargetId);

                default:
                    throw new ArgumentOutOfRangeException(nameof(effectData.Effect), effectData.Effect, null);
            }

            return null;
        }

        private void TryRevealRuntimeStatement(string statementId)
        {
            if (string.IsNullOrWhiteSpace(statementId))
            {
                return;
            }

            if (caseRuntime.TryGetStatement(statementId, out StatementRuntime statement))
            {
                statement.IsVisible = true;
            }
        }

        private void TryRevealRuntimeTestimony(string testimonyId)
        {
            if (string.IsNullOrWhiteSpace(testimonyId))
            {
                return;
            }

            if (caseRuntime.TryGetTestimony(testimonyId, out TestimonyRuntime testimony))
            {
                foreach (StatementRuntime statement in testimony.Statements)
                {
                    statement.IsVisible = true;
                }
            }
        }

        private void TryRevealRuntimeWitness(string witnessId)
        {
            if (string.IsNullOrWhiteSpace(witnessId))
            {
                return;
            }

            if (caseRuntime.TryGetWitness(witnessId, out WitnessRuntime witness))
            {
                foreach (TestimonyRuntime testimony in witness.Testimonies)
                {
                    foreach (StatementRuntime statement in testimony.Statements)
                    {
                        statement.IsVisible = true;
                    }

                    // Persist the witness's testimony IDs as revealed so saves can restore this state.
                    courtState.RevealTestimony(testimony.Data.Id);
                }
            }
        }
    }
}
