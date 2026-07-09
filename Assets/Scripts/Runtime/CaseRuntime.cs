using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class CaseRuntime
    {
        public CaseRuntime(
            CaseData data,
            IReadOnlyList<EvidenceRuntime> evidence,
            IReadOnlyList<WitnessRuntime> witnesses,
            CourtStateRuntime courtState)
        {
            Data = data;
            Evidence = evidence;
            Witnesses = witnesses;
            CourtState = courtState;
        }

        public CaseData Data { get; }

        public IReadOnlyList<EvidenceRuntime> Evidence { get; }

        public IReadOnlyList<WitnessRuntime> Witnesses { get; }

        public CourtStateRuntime CourtState { get; }
    }
}
