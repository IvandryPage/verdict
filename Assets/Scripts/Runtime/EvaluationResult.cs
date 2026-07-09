using Verdict.Data.Cases;

namespace Verdict.Systems
{
    public sealed class EvaluationResult
    {
        public bool IsSuccess { get; }

        public ClaimData MatchedClaim { get; }

        public EvaluationRuleData MatchedRule { get; }

        public bool HasMatchedRule => MatchedRule != null;

        public EvaluationResult(
            bool isSuccess,
            ClaimData matchedClaim,
            EvaluationRuleData matchedRule)
        {
            IsSuccess = isSuccess;
            MatchedClaim = matchedClaim;
            MatchedRule = matchedRule;
        }
    }
}
