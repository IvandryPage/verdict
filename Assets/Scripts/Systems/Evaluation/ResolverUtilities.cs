using System.Collections.Generic;
using System.Linq;
using Verdict.Common.Comparisons;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;
using Verdict.Runtime;

namespace Verdict.Systems.Evaluation
{
    /// <summary>
    /// Interprets ArgumentConditionData against a ResolverContext.
    /// Condition data classes carry no behavior themselves (Data layer
    /// stays free of Runtime/Systems dependencies) - all evaluation logic
    /// lives here.
    /// </summary>
    public static class ResolverUtilities
    {
        public static bool EvaluateAll(
            IReadOnlyList<ArgumentConditionData> conditions,
            ResolverContext context)
        {
            foreach (ArgumentConditionData condition in conditions)
            {
                if (!Evaluate(condition, context))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Evaluate(
            ArgumentConditionData condition,
            ResolverContext context)
        {
            return condition switch
            {
                ActionConditionData action =>
                    context.Argument.Action == action.RequiredAction,

                EvidenceConditionData evidence =>
                    EvaluateEvidence(evidence, context),

                FactConditionData fact =>
                    EvaluateFact(fact, context),

                CourtStateConditionData courtState =>
                    Comparison.Evaluate(
                        context.Case.CourtState.GetCourtStat(courtState.Stat),
                        courtState.Operator,
                        courtState.Value),

                CharacterConditionData character =>
                    EvaluateCharacter(character, context),

                ClaimConditionData claim =>
                    EvaluateClaim(claim, context),

                ArgumentContextConditionData argumentContext =>
                    EvaluateArgumentContext(argumentContext, context),

                _ => false
            };
        }

        private static bool EvaluateArgumentContext(
            ArgumentContextConditionData condition,
            ResolverContext context)
        {
            if (string.IsNullOrWhiteSpace(condition.ContextKey))
            {
                return false;
            }

            return context.Argument.AdditionalContext.TryGetValue(
                    condition.ContextKey,
                    out string value) &&
                value == condition.RequiredValue;
        }

        private static bool EvaluateEvidence(
            EvidenceConditionData condition,
            ResolverContext context)
        {
            return condition.RequiredEvidence != null &&
                   context.Argument.Evidence != null &&
                   context.Argument.Evidence == condition.RequiredEvidence;
        }

        private static bool EvaluateFact(
            FactConditionData condition,
            ResolverContext context)
        {
            FactData fact = FindFact(context.Case, condition.FactId);

            if (fact == null)
            {
                return false;
            }

            if (!condition.RequireSupportingEvidencePresented)
            {
                return true;
            }

            EvidenceData presented = context.Argument.Evidence;

            return presented != null &&
                   fact.SupportingEvidence.Any(e => e == presented);
        }

        private static bool EvaluateCharacter(
            CharacterConditionData condition,
            ResolverContext context)
        {
            if (condition.Character == null)
            {
                return false;
            }

            foreach (WitnessRuntime witness in context.Case.Witnesses)
            {
                if (witness.Character.Data == condition.Character)
                {
                    int current = witness.GetCharacterStat(condition.Stat);
                    return Comparison.Evaluate(current, condition.Operator, condition.Value);
                }
            }

            return false;
        }

        private static bool EvaluateClaim(
            ClaimConditionData condition,
            ResolverContext context)
        {
            if (!context.Case.TryGetClaim(condition.ClaimId, out ClaimRuntime claim))
            {
                return false;
            }

            return condition.RequiredState switch
            {
                ClaimRequiredState.ResolvedSuccessfully => claim.IsResolved && claim.WasSuccessful,
                ClaimRequiredState.ResolvedAsFailed => claim.IsResolved && !claim.WasSuccessful,
                ClaimRequiredState.NotResolved => !claim.IsResolved,
                ClaimRequiredState.Attempted => claim.HasBeenAttempted,
                ClaimRequiredState.NotAttempted => !claim.HasBeenAttempted,
                _ => false
            };
        }

        public static FactData FindFact(CaseRuntime caseRuntime, string factId)
        {
            if (string.IsNullOrWhiteSpace(factId) || caseRuntime.Data.Truth == null)
            {
                return null;
            }

            return caseRuntime.Data.Truth.Facts
                .FirstOrDefault(f => f.Id == factId);
        }
    }
}
