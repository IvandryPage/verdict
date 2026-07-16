using System;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class StatementContext
    {
        public CaseData Case { get; }

        public WitnessData Witness { get; }

        public TestimonyData Testimony { get; }

        public StatementData Statement { get; }

        public int WitnessIndex { get; }

        public int TestimonyIndex { get; }

        public int StatementIndex { get; }

        public StatementContext(
            CaseData caseData,
            WitnessData witness,
            TestimonyData testimony,
            StatementData statement,
            int witnessIndex,
            int testimonyIndex,
            int statementIndex)
        {
            Case = caseData;

            Witness = witness;
            Testimony = testimony;
            Statement = statement;

            WitnessIndex = witnessIndex;
            TestimonyIndex = testimonyIndex;
            StatementIndex = statementIndex;
        }

        public string StatementPropertyPath =>
            $"witnesses.Array.data[{WitnessIndex}]." +
            $"testimonies.Array.data[{TestimonyIndex}]." +
            $"statements.Array.data[{StatementIndex}]";

        public string ClaimPropertyPath(
            ClaimData claim)
        {
            int index = FindClaimIndex(claim);

            return index < 0
                ? null
                : $"{StatementPropertyPath}.claims.Array.data[{index}]";
        }

        public string EvaluationRulePropertyPath(
            ClaimData claim,
            EvaluationRuleData rule)
        {
            int claimIndex = FindClaimIndex(claim);

            if (claimIndex < 0)
                return null;

            int ruleIndex = FindRuleIndex(claim, rule);

            if (ruleIndex < 0)
                return null;

            return
                $"{StatementPropertyPath}.claims.Array.data[{claimIndex}]." +
                $"evaluationRules.Array.data[{ruleIndex}]";
        }

        public string SuccessEffectPropertyPath(
            ClaimData claim,
            EvaluationRuleData rule,
            CourtStateEffectData effect)
        {
            int claimIndex = FindClaimIndex(claim);

            if (claimIndex < 0)
                return null;

            int ruleIndex = FindRuleIndex(claim, rule);

            if (ruleIndex < 0)
                return null;

            int effectIndex =
                FindSuccessEffectIndex(rule, effect);

            if (effectIndex < 0)
                return null;

            return
                $"{StatementPropertyPath}.claims.Array.data[{claimIndex}]." +
                $"evaluationRules.Array.data[{ruleIndex}]." +
                $"successEffects.Array.data[{effectIndex}]";
        }

        public string FailureEffectPropertyPath(
            ClaimData claim,
            EvaluationRuleData rule,
            CourtStateEffectData effect)
        {
            int claimIndex = FindClaimIndex(claim);

            if (claimIndex < 0)
                return null;

            int ruleIndex = FindRuleIndex(claim, rule);

            if (ruleIndex < 0)
                return null;

            int effectIndex =
                FindFailureEffectIndex(rule, effect);

            if (effectIndex < 0)
                return null;

            return
                $"{StatementPropertyPath}.claims.Array.data[{claimIndex}]." +
                $"evaluationRules.Array.data[{ruleIndex}]." +
                $"failureEffects.Array.data[{effectIndex}]";
        }

        private int FindClaimIndex(
            ClaimData claim)
        {
            for (int i = 0; i < Statement.Claims.Count; i++)
            {
                if (ReferenceEquals(
                        Statement.Claims[i],
                        claim))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindRuleIndex(
            ClaimData claim,
            EvaluationRuleData rule)
        {
            for (int i = 0; i < claim.EvaluationRules.Count; i++)
            {
                if (ReferenceEquals(
                        claim.EvaluationRules[i],
                        rule))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindSuccessEffectIndex(
            EvaluationRuleData rule,
            CourtStateEffectData effect)
        {
            for (int i = 0; i < rule.SuccessEffects.Count; i++)
            {
                if (ReferenceEquals(
                        rule.SuccessEffects[i],
                        effect))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindFailureEffectIndex(
            EvaluationRuleData rule,
            CourtStateEffectData effect)
        {
            for (int i = 0; i < rule.FailureEffects.Count; i++)
            {
                if (ReferenceEquals(
                        rule.FailureEffects[i],
                        effect))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
