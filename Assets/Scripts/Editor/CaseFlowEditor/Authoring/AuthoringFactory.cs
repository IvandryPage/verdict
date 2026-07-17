using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow.Authoring
{
    /// <summary>
    /// Creates authoring data with sensible defaults.
    /// Used exclusively by the Verdict Editor.
    /// </summary>
    internal static class AuthoringFactory
    {
        public static WitnessData CreateWitness()
        {
            return new WitnessData(
                AuthoringId.New());
        }

        public static TestimonyData CreateTestimony()
        {
            return new TestimonyData(
                AuthoringId.New(),
                AuthoringDefaults.TestimonyTitle,
                AuthoringDefaults.TestimonyDescription);
        }

        public static StatementData CreateStatement()
        {
            return new StatementData(
                AuthoringId.New(),
                AuthoringDefaults.StatementText);
        }

        public static ClaimData CreateClaim()
        {
            return new ClaimData(
                AuthoringId.New());
        }

        public static EvaluationRuleData CreateEvaluationRule()
        {
            return new EvaluationRuleData();
        }

        public static CourtStateEffectData CreateEffect()
        {
            return new CourtStateEffectData();
        }
    }
}
