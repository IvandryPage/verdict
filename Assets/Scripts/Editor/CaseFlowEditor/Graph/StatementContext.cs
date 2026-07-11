using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class StatementContext
    {
        public StatementData Statement { get; }

        public TestimonyData Testimony { get; }

        public WitnessData Witness { get; }

        public StatementContext(
            StatementData statement,
            TestimonyData testimony,
            WitnessData witness)
        {
            Statement = statement;
            Testimony = testimony;
            Witness = witness;
        }
    }
}
