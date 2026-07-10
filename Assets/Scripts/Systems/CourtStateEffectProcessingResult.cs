using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Systems
{
    public sealed class CourtStateEffectProcessingResult
    {
        public CourtStateEffectProcessingResult(
            IReadOnlyList<CourtStateEffectIntent> intents)
        {
            Intents = intents;
        }

        public IReadOnlyList<CourtStateEffectIntent> Intents { get; }
    }

    public sealed class CourtStateEffectIntent
    {
        public CourtStateEffectIntent(
            CourtStateEffect effect,
            string targetId = null)
        {
            Effect = effect;
            TargetId = targetId;
        }

        public CourtStateEffect Effect { get; }

        public string TargetId { get; }
    }
}
