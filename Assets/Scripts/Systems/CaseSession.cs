using System;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CaseSession
    {
        public CaseSession(
            CaseRuntime runtime,
            CourtroomFlow flow,
            NarrativeCoordinator narrativeCoordinator,
            EvaluationSystem evaluationSystem,
            CourtStateEffectProcessor effectProcessor)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            Flow = flow ?? throw new ArgumentNullException(nameof(flow));
            NarrativeCoordinator = narrativeCoordinator ?? throw new ArgumentNullException(nameof(narrativeCoordinator));
            EvaluationSystem = evaluationSystem ?? throw new ArgumentNullException(nameof(evaluationSystem));
            EffectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        }

        public CaseRuntime Runtime { get; }

        public CourtroomFlow Flow { get; }

        public NarrativeCoordinator NarrativeCoordinator { get; }

        public EvaluationSystem EvaluationSystem { get; }

        public CourtStateEffectProcessor EffectProcessor { get; }

        public CourtStateRuntime CourtState => Runtime.CourtState;
    }
}
