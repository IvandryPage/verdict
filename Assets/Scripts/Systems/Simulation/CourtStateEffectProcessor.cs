using System;
using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CourtStateEffectProcessor
    {
        private readonly CaseRuntime runtime;

        public CourtStateEffectProcessor(
            CaseRuntime runtime)
        {
            this.runtime =
                runtime ??
                throw new ArgumentNullException(nameof(runtime));
        }

        private CourtStateRuntime CourtState => runtime.CourtState;
        private CaseRuntime Runtime => runtime;

        public CourtStateEffectProcessingResult Apply(
            ResolverResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            ApplyClaimLifecycle(result);

            if (!result.HasResolvedClaims)
            {
                return new CourtStateEffectProcessingResult(
                    Array.Empty<CourtStateEffectIntent>());
            }

            List<CourtStateEffectIntent> intents = new();

            foreach (CourtStateEffectData effect in result.GeneratedEffects)
            {
                if (effect == null)
                    continue;

                CourtStateEffectIntent intent =
                    ApplyEffect(effect);

                if (intent != null)
                    intents.Add(intent);
            }

            return new CourtStateEffectProcessingResult(intents);
        }

        /// <summary>
        /// The only place a Claim's runtime lifecycle changes. A claim
        /// only ever locks (IsResolved = true) on success - failed
        /// attempts just increment AttemptCount, so the player can keep
        /// trying different arguments against the same contradiction.
        /// A single argument can resolve several claims at once, so
        /// every entry in ResolvedClaims is applied.
        /// </summary>
        private static void ApplyClaimLifecycle(ResolverResult result)
        {
            foreach (ResolvedClaim resolved in result.ResolvedClaims)
            {
                ClaimRuntime claim = resolved.Claim;

                if (claim == null)
                {
                    continue;
                }

                claim.HasBeenAttempted = true;
                claim.AttemptCount++;

                if (resolved.IsSuccess)
                {
                    claim.IsResolved = true;
                    claim.WasSuccessful = true;
                }
            }
        }

        private CourtStateEffectIntent ApplyEffect(
            CourtStateEffectData effectData)
        {
            switch (effectData.Effect)
            {
                case CourtStateEffect.None:
                    break;

                case CourtStateEffect.RevealStatement:

                    CourtState.RevealStatement(effectData.TargetId);
                    RevealStatement(effectData.TargetId);

                    break;

                case CourtStateEffect.RevealTestimony:

                    CourtState.RevealTestimony(effectData.TargetId);
                    RevealTestimony(effectData.TargetId);

                    break;

                case CourtStateEffect.RevealWitness:

                    RevealWitness(effectData.TargetId);

                    break;

                case CourtStateEffect.UnlockEvidence:

                    CourtState.UnlockEvidence(effectData.TargetId);

                    break;

                case CourtStateEffect.ModifyCourtStat:

                    CourtState.ModifyCourtStat(
                        effectData.CourtStat,
                        effectData.Value,
                        effectData.Operation);

                    break;

                case CourtStateEffect.ModifyCharacterStat:

                    if (Runtime.TryGetWitness(
                        effectData.TargetId,
                        out WitnessRuntime witness))
                    {
                        witness.ModifyCharacterStat(
                            effectData.CharacterStat,
                            effectData.Value,
                            effectData.Operation);
                    }

                    break;

                case CourtStateEffect.JumpStatement:
                case CourtStateEffect.JumpTestimony:
                case CourtStateEffect.JumpWitness:

                    return new CourtStateEffectIntent(
                        effectData.Effect,
                        effectData.TargetId);

                default:

                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private void RevealStatement(string statementId)
        {
            if (Runtime.TryGetStatement(
                statementId,
                out StatementRuntime statement))
            {
                statement.IsVisible = true;
            }
        }

        private void RevealTestimony(string testimonyId)
        {
            if (!Runtime.TryGetTestimony(
                testimonyId,
                out TestimonyRuntime testimony))
            {
                return;
            }

            foreach (StatementRuntime statement in testimony.Statements)
            {
                statement.IsVisible = true;
            }
        }

        private void RevealWitness(string witnessId)
        {
            if (!Runtime.TryGetWitness(
                witnessId,
                out WitnessRuntime witness))
            {
                return;
            }

            witness.IsVisible = true;

            foreach (TestimonyRuntime testimony in witness.Testimonies)
            {
                CourtState.RevealTestimony(testimony.Data.Id);

                foreach (StatementRuntime statement in testimony.Statements)
                {
                    statement.IsVisible = true;
                }
            }
        }
    }
}
