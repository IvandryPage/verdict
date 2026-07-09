using Verdict.Data.Evidence;

namespace Verdict.Runtime
{
    public sealed class EvidenceRuntime
    {
        public EvidenceRuntime(EvidenceData data)
        {
            Data = data;
        }

        public EvidenceData Data { get; }

        public bool IsCollected { get; set; }

        public bool IsUnlocked { get; set; }

        public bool HasBeenPresented { get; set; }
    }
}
