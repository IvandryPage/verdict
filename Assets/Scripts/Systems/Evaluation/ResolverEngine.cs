using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Runtime;

namespace Verdict.Systems.Evaluation
{
    /// <summary>
    /// Evaluates a player argument against the claims it could apply to.
    ///
    /// The ResolverEngine does not modify gameplay state directly.
    /// It only determines whether an action succeeds or fails and
    /// generates the effects that should be applied afterward.
    ///
    /// Pipeline:
    ///
    /// PlayerArgument
    ///      ↓
    /// Find Statement
    ///      ↓
    /// Find Claims
    ///      ↓
    /// Find Rule by PlayerAction
    ///      ↓
    /// Evaluate Conditions
    ///      ↓
    /// Generate ResolverResult
    /// </summary>
    public sealed class ResolverEngine
    {
        private readonly CourtroomFlow courtroomFlow;

        public ResolverEngine(
            CourtroomFlow courtroomFlow)
        {
            this.courtroomFlow =
                courtroomFlow ??
                throw new ArgumentNullException(
                    nameof(courtroomFlow));
        }

        public ResolverResult Resolve(
            PlayerArgumentData argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(
                    nameof(argument));
            }

            CaseRuntime caseRuntime =
                courtroomFlow.Runtime;

            var diagnostics =
                new List<string>();


            StatementRuntime statement =
                FindStatement(
                    caseRuntime,
                    argument);

            if (statement == null)
            {
                diagnostics.Add(
                    "No statement to resolve against.");

                return new ResolverResult(
                    false,
                    Array.Empty<ResolvedClaim>(),
                    Array.Empty<CourtStateEffectData>(),
                    diagnostics);
            }


            ResolverContext context =
                new(
                    caseRuntime,
                    statement,
                    argument);


            IEnumerable<ClaimRuntime> claims =
                FindClaims(
                    statement,
                    argument);

            var resolvedClaims =
                new List<ResolvedClaim>();

            var generatedEffects =
                new List<CourtStateEffectData>();


            foreach (ClaimRuntime claim in claims)
            {
                if (!claim.CanResolve)
                {
                    diagnostics.Add(
                        $"Claim '{claim.Data.Id}' skipped - " +
                        "already resolved.");

                    continue;
                }

                ArgumentRuleData rule =
                    claim.Data.ArgumentRules
                        .FirstOrDefault(
                            r => r.Action == argument.Action);

                if (rule == null)
                {
                    diagnostics.Add(
                        $"Claim '{claim.Data.Id}' has no rule " +
                        $"for action '{argument.Action}'.");

                    continue;
                }

                bool success =
                    ResolverUtilities.EvaluateAll(
                        rule.Conditions,
                        context);

                diagnostics.Add(
                    success
                        ? $"Claim '{claim.Data.Id}': " +
                          $"action '{argument.Action}' matched " +
                          $"and all {rule.Conditions.Count} " +
                          "condition(s) passed."
                        : $"Claim '{claim.Data.Id}': " +
                          $"action '{argument.Action}' matched " +
                          "but conditions failed.");

                resolvedClaims.Add(
                    new ResolvedClaim(
                        claim,
                        rule,
                        success));

                IReadOnlyList<CourtStateEffectData> effects =
                    success
                        ? rule.SuccessEffects
                        : rule.FailureEffects;

                if (effects != null)
                {
                    generatedEffects.AddRange(
                        effects);
                }
            }


            if (resolvedClaims.Count == 0)
            {
                diagnostics.Add(
                    $"No claim had a rule matching " +
                    $"action '{argument.Action}'.");
            }

            if (argument.Action == PlayerAction.RemainSilent)
            {
                IReadOnlyList<CourtStateEffectData> silenceCosts =
                    CreateSilentCostEffects();

                if (silenceCosts.Count > 0)
                {
                    generatedEffects.AddRange(silenceCosts);
                    diagnostics.Add(
                        "RemainSilent applied a cost to court state.");
                }
            }

            bool overallSuccess =
                resolvedClaims.Any(
                    rc => rc.IsSuccess);


            return new ResolverResult(
                overallSuccess,
                resolvedClaims,
                generatedEffects,
                diagnostics);
        }

        private static IReadOnlyList<CourtStateEffectData> CreateSilentCostEffects()
        {
            var choices = new[]
            {
                (CourtStat.JudgeTrust, -5, StatOperation.Add),
                (CourtStat.PublicOpinion, -4, StatOperation.Add),
                (CourtStat.DefenseConfidence, -4, StatOperation.Add),
                (CourtStat.ProsecutorPressure, 5, StatOperation.Add)
            };

            int index = UnityEngine.Random.Range(0, choices.Length);
            var chosen = choices[index];

            CourtStateEffectData cost = new CourtStateEffectData();
            cost.SetEffect(CourtStateEffect.ModifyCourtStat);
            cost.SetCourtStat(chosen.Item1);
            cost.SetOperation(chosen.Item3);
            cost.SetValue(chosen.Item2);

            return new[] { cost };
        }

        private StatementRuntime FindStatement(
            CaseRuntime caseRuntime,
            PlayerArgumentData argument)
        {
            if (argument.SelectedStatement != null &&
                caseRuntime.TryGetStatement(
                    argument.SelectedStatement.Id,
                    out StatementRuntime selected))
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
                ClaimRuntime match =
                    statement.Claims.FirstOrDefault(
                        c => c.Data == argument.SelectedClaim);

                if (match != null)
                {
                    return new[] { match };
                }
            }

            return statement.Claims;
        }
    }
}
