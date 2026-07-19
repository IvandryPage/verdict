using Verdict.Data.Cases;

namespace Verdict.Editor.CaseEditor.Authoring
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

        public static FactData CreateFact()
        {
            return new FactData(
                AuthoringId.New());
        }

        public static ArgumentRuleData CreateArgumentRule()
        {
            return new ArgumentRuleData();
        }

        public static CourtStateEffectData CreateEffect()
        {
            return new CourtStateEffectData();
        }

        public static ActionConditionData CreateActionCondition() => new();

        public static EvidenceConditionData CreateEvidenceCondition() => new();

        public static FactConditionData CreateFactCondition() => new();

        public static CourtStateConditionData CreateCourtStateCondition() => new();

        public static CharacterConditionData CreateCharacterCondition() => new();

        public static ClaimConditionData CreateClaimCondition() => new();

        public static ArgumentContextConditionData CreateArgumentContextCondition() => new();
    }
}
