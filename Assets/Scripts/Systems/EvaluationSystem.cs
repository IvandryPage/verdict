using System;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class EvaluationSystem
    {
        private readonly CourtroomFlow courtroomFlow;

        public EvaluationSystem(CourtroomFlow courtroomFlow)
        {
            this.courtroomFlow = courtroomFlow
                ?? throw new ArgumentNullException(nameof(courtroomFlow));
        }

        public EvaluationResult Evaluate(
            EvaluationType evaluationType,
            EvidenceData presentedEvidence = null)
        {
            StatementRuntime currentStatement = courtroomFlow.CurrentStatement;

            ClaimData failureClaim = null;
            EvaluationRuleData failureRule = null;

            foreach (ClaimData claim in currentStatement.Data.Claims)
            {
                foreach (EvaluationRuleData rule in claim.EvaluationRules)
                {
                    if (rule.EvaluationType != evaluationType)
                    {
                        continue;
                    }

                    if (Matches(rule, presentedEvidence))
                    {
                        return new EvaluationResult(
                            true,
                            claim,
                            rule);
                    }

                    // Simpan kandidat failure pertama.
                    failureClaim ??= claim;
                    failureRule ??= rule;
                }
            }

            return new EvaluationResult(
                false,
                failureClaim,
                failureRule);
        }

        private static bool Matches(
            EvaluationRuleData rule,
            EvidenceData presentedEvidence)
        {
            switch (rule.EvaluationType)
            {
                case EvaluationType.PresentEvidence:
                    return rule.RequiredEvidence == presentedEvidence;

                case EvaluationType.Press:
                case EvaluationType.Question:
                case EvaluationType.RemainSilent:
                    return true;

                default:
                    return false;
            }
        }
    }
}
