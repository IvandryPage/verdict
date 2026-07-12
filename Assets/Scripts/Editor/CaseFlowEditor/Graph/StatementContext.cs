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
    }
}
