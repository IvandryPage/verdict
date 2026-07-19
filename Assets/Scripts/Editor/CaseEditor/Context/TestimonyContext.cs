using Verdict.Data.Cases;

namespace Verdict.Editor.CaseEditor
{
    public sealed class TestimonyContext
    {
        public CaseData Case { get; }

        public WitnessData Witness { get; }

        public TestimonyData Testimony { get; }


        public int WitnessIndex { get; }

        public int TestimonyIndex { get; }


        public TestimonyContext(
            CaseData caseData,
            WitnessData witness,
            TestimonyData testimony,
            int witnessIndex,
            int testimonyIndex)
        {
            Case = caseData;

            Witness = witness;

            Testimony = testimony;

            WitnessIndex = witnessIndex;

            TestimonyIndex = testimonyIndex;
        }


        public string PropertyPath =>
            $"witnesses.Array.data[{WitnessIndex}]." +
            $"testimonies.Array.data[{TestimonyIndex}]";
    }
}
