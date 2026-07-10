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
                    courtState.ModifyCourtStat(effectData.CourtStat, effectData.Value);
                    break;

                case CourtStateEffect.ModifyCharacterStat:
                    // Find the witness/character runtime by target id and modify the stat.
                    if (!string.IsNullOrWhiteSpace(effectData.TargetId))
                    {
                        foreach (WitnessRuntime witness in caseRuntime.Witnesses)
                        {
                            if (witness.Data.Id == effectData.TargetId)
                            {
                                witness.ModifyCharacterStat(effectData.CharacterStat, effectData.Value);
                                break;
                            }
                        }
                    }

                    break;

                case CourtStateEffect.TriggerEnding:
                    // Handled by EndingResolver / CourtroomController.
                    break;

                case CourtStateEffect.JumpStatement:
                case CourtStateEffect.JumpTestimony:
                case CourtStateEffect.JumpWitness:
                    // Flow-level jumps are handled by the controller/flow system; the effect can be
                    // interpreted by higher-level systems which have access to the flow controller.
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

        private void TryRevealRuntimeTestimony(string testimonyId)
        {
            if (string.IsNullOrWhiteSpace(testimonyId))
            {
                return;
            }

            foreach (WitnessRuntime witness in caseRuntime.Witnesses)
            {
                foreach (TestimonyRuntime testimony in witness.Testimonies)
                {
                    if (testimony.Data.Id == testimonyId)
                    {
                        foreach (StatementRuntime statement in testimony.Statements)
                        {
                            statement.IsVisible = true;
                        }

                        return;
                    }
                }
            }
        }

        private void TryRevealRuntimeWitness(string witnessId)
        {
            if (string.IsNullOrWhiteSpace(witnessId))
            {
                return;
            }

            foreach (WitnessRuntime witness in caseRuntime.Witnesses)
            {
                if (witness.Data.Id == witnessId)
                {
                    foreach (TestimonyRuntime testimony in witness.Testimonies)
                    {
                        foreach (StatementRuntime statement in testimony.Statements)
                        {
                            statement.IsVisible = true;
                        }
                    }

                    // Persist the witness's testimony IDs as revealed so saves can restore this state.
                    foreach (TestimonyRuntime testimony in witness.Testimonies)
                    {
                        courtState.RevealTestimony(testimony.Data.Id);
                    }

                    return;
                }
            }
        }
    }
}
