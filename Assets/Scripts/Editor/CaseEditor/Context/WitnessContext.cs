using Verdict.Data.Cases;

namespace Verdict.Editor.CaseEditor
{
    public sealed class WitnessContext
    {
        public CaseData Case { get; }

        public WitnessData Witness { get; }

        public int WitnessIndex { get; }


        public WitnessContext(
            CaseData caseData,
            WitnessData witness,
            int index)
        {
            Case = caseData;
            Witness = witness;
            WitnessIndex = index;
        }


        public string PropertyPath =>
            $"witnesses.Array.data[{WitnessIndex}]";
    }
}
