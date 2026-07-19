using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Systems.Evaluation
{
    /// <summary>
    /// Evaluates a player argument against the claims it could apply to,
    /// and returns a ResolverResult - it never modifies gameplay state
    /// directly. CourtStateEffectProcessor applies GeneratedEffects (and
    /// updates claim lifecycle) afterward.
    ///
    /// Pipeline: PlayerArgument -> Find Statement -> Find Claims ->
    /// Find Rules -> Evaluate Conditions -> Choose Matching Rules ->
    /// Generate ResolverResult.
    /// </summary>
    public sealed class ResolverEngine
    {
        private readonly CourtroomFlow courtroomFlow;

        public ResolverEngine(CourtroomFlow courtroomFlow)
        {
            this.courtroomFlow =
                courtroomFlow ??
                throw new ArgumentNullException(nameof(courtroomFlow));
        }

        public ResolverResult Resolve(PlayerArgumentData argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            CaseRuntime caseRuntime = courtroomFlow.Runtime;

            // Find Statement
            StatementRuntime statement = FindStatement(caseRuntime, argument);

            var diagnostics = new List<string>();

            if (statement == null)
            {
                diagnostics.Add("No statement to resolve against.");
                return new ResolverResult(false, Array.Empty<ResolvedClaim>(), Array.Empty<CourtStateEffectData>(), diagnostics);
            }

            ResolverContext context = new(caseRuntime, statement, argument);

            // Find Claims (narrowed to a single claim if one was selected)
            IEnumerable<ClaimRuntime> claims = FindClaims(statement, argument);

            var resolvedClaims = new List<ResolvedClaim>();
            var generatedEffects = new List<CourtStateEffectData>();

            foreach (ClaimRuntime claim in claims)
            {
                if (!claim.CanResolve)
                {
                    diagnostics.Add($"Claim '{claim.Data.Id}' skipped - already resolved.");
                    continue;
                }

                // Find Rules (matching this argument's action)
                ArgumentRuleData rule = claim.Data.ArgumentRules
                    .FirstOrDefault(r => r.Action == argument.Action);

                if (rule == null)
                {
                    continue;
                }

                // Evaluate Conditions
                bool success = ResolverUtilities.EvaluateAll(rule.Conditions, context);

                diagnostics.Add(
                    success
                        ? $"Claim '{claim.Data.Id}': rule matched, all {rule.Conditions.Count} condition(s) passed."
                        : $"Claim '{claim.Data.Id}': rule action matched but conditions failed.");

                // Choose Matching Rule for this claim
                resolvedClaims.Add(new ResolvedClaim(claim, rule, success));

                IReadOnlyList<CourtStateEffectData> effects =
                    success ? rule.SuccessEffects : rule.FailureEffects;

                generatedEffects.AddRange(effects);
            }

            if (resolvedClaims.Count == 0)
            {
                diagnostics.Add("No claim had a rule matching this action.");
            }

            bool overallSuccess = resolvedClaims.Any(rc => rc.IsSuccess);

            // Generate ResolverResult
            return new ResolverResult(
                overallSuccess,
                resolvedClaims,
                generatedEffects,
                diagnostics);
        }

        private StatementRuntime FindStatement(
            CaseRuntime caseRuntime,
            PlayerArgumentData argument)
        {
            if (argument.SelectedStatement != null &&
                caseRuntime.TryGetStatement(argument.SelectedStatement.Id, out StatementRuntime selected))
            {
                return selected;
            }

            return courtroomFlow.CurrentStatement;
        }

        private static IEnumerable<ClaimRuntime> FindClaims(
            StatementRuntime statement,
            PlayerArgumentData argument)
        {
            if (argument.SelectedClaim != null)
            {
                ClaimRuntime match = statement.Claims
                    .FirstOrDefault(c => c.Data == argument.SelectedClaim);

                if (match != null)
                {
                    return new[] { match };
                }
            }

            return statement.Claims;
        }
    }
}
