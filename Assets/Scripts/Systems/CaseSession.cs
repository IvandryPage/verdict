using System;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CaseSession
    {
        public CaseSession(
            CaseRuntime runtime,
            CourtroomFlow flow,
            DialogueRunner dialogueRunner,
            EvaluationSystem evaluationSystem,
            CourtStateEffectProcessor effectProcessor)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            Flow = flow ?? throw new ArgumentNullException(nameof(flow));
            DialogueRunner = dialogueRunner ?? throw new ArgumentNullException(nameof(dialogueRunner));
            EvaluationSystem = evaluationSystem ?? throw new ArgumentNullException(nameof(evaluationSystem));
            EffectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        }

        public CaseRuntime Runtime { get; }

        public CourtroomFlow Flow { get; }

        public DialogueRunner DialogueRunner { get; }

        public EvaluationSystem EvaluationSystem { get; }

        public CourtStateEffectProcessor EffectProcessor { get; }

        public CourtStateRuntime CourtState => Runtime.CourtState;
    }
}
