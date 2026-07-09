using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Runtime.Court;
using Verdict.Runtime.Evidence;
using Verdict.Runtime.Witnesses;

namespace Verdict.Runtime.Cases
{
    public sealed class CaseRuntime
    {
        public CaseRuntime(
            CaseData data,
            IReadOnlyList<EvidenceRuntime> evidence,
            IReadOnlyList<WitnessRuntime> witnesses)
        {
            Data = data;
            Evidence = evidence;
            Witnesses = witnesses;

            CourtState = new CourtStateRuntime();
        }

        public CaseData Data { get; }

        public IReadOnlyList<EvidenceRuntime> Evidence { get; }

        public IReadOnlyList<WitnessRuntime> Witnesses { get; }

        public CourtStateRuntime CourtState { get; }

        public int CurrentWitnessIndex { get; set; }
    }
}
