using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class WitnessRuntime
    {
        public WitnessRuntime(
            WitnessData data,
            IReadOnlyList<TestimonyRuntime> testimonies)
        {
            Data = data;
            Testimonies = testimonies;
        }

        public WitnessData Data { get; }

        public IReadOnlyList<TestimonyRuntime> Testimonies { get; }

        public float Credibility { get; set; } = 100f;
    }
}
