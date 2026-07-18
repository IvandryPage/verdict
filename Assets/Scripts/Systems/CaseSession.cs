using System;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CaseSession
    {
        public CaseSession(
            CaseRuntime runtime,
            CourtroomFlow flow,
            DialogueCoordinator dialogueCoordinator,
            EvaluationSystem evaluationSystem,
            CourtStateEffectProcessor effectProcessor)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            Flow = flow ?? throw new ArgumentNullException(nameof(flow));
            DialogueCoordinator = dialogueCoordinator ?? throw new ArgumentNullException(nameof(dialogueCoordinator));
            EvaluationSystem = evaluationSystem ?? throw new ArgumentNullException(nameof(evaluationSystem));
            EffectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        }

        public CaseRuntime Runtime { get; }

        public CourtroomFlow Flow { get; }

        public DialogueCoordinator DialogueCoordinator {get;}

        public EvaluationSystem EvaluationSystem { get; }

        public CourtStateEffectProcessor EffectProcessor { get; }

        public CourtStateRuntime CourtState => Runtime.CourtState;
    }
}
