using System;
using Verdict.Runtime;
using Verdict.Systems.Evaluation;

namespace Verdict.Systems
{
    public sealed class CaseSession
    {
        public CaseSession(
            CaseRuntime runtime,
            CourtroomFlow flow,
            NarrativeCoordinator narrativeCoordinator,
            ResolverEngine resolverEngine,
            CourtStateEffectProcessor effectProcessor)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            Flow = flow ?? throw new ArgumentNullException(nameof(flow));
            NarrativeCoordinator = narrativeCoordinator ?? throw new ArgumentNullException(nameof(narrativeCoordinator));
            ResolverEngine = resolverEngine ?? throw new ArgumentNullException(nameof(resolverEngine));
            EffectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        }

        public CaseRuntime Runtime { get; }

        public CourtroomFlow Flow { get; }

        public NarrativeCoordinator NarrativeCoordinator { get; }

        public ResolverEngine ResolverEngine { get; }

        public CourtStateEffectProcessor EffectProcessor { get; }

        public CourtStateRuntime CourtState => Runtime.CourtState;
    }
}
