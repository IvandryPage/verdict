using System;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CourtroomController
    {
        private readonly CaseSessionManager caseSessionManager;
        private readonly CourtroomFlow courtroomFlow;
        private readonly EvaluationSystem evaluationSystem;
        private readonly CourtStateEffectProcessor effectProcessor;

        public event Action<EvaluationResult> EvaluationCompleted;
        public event Action<StatementRuntime> CurrentStatementChanged;

        public CourtroomController(
            CaseSessionManager caseSessionManager,
            CourtroomFlow courtroomFlow,
            EvaluationSystem evaluationSystem,
            CourtStateEffectProcessor effectProcessor)
        {
            this.caseSessionManager = caseSessionManager
                ?? throw new ArgumentNullException(nameof(caseSessionManager));

            this.courtroomFlow = courtroomFlow
                ?? throw new ArgumentNullException(nameof(courtroomFlow));

            this.evaluationSystem = evaluationSystem
                ?? throw new ArgumentNullException(nameof(evaluationSystem));

            this.effectProcessor = effectProcessor
                ?? throw new ArgumentNullException(nameof(effectProcessor));
        }

        public void BeginCase()
        {
            // Reserved for future initialization.
        }

        public EvaluationResult PresentEvidence(EvidenceData evidence)
        {
            return Execute(EvaluationType.PresentEvidence, evidence);
        }

        public EvaluationResult Press()
        {
            return Execute(EvaluationType.Press);
        }

        public EvaluationResult Question()
        {
            return Execute(EvaluationType.Question);
        }

        public EvaluationResult RemainSilent()
        {
            return Execute(EvaluationType.RemainSilent);
        }

        public bool MoveNextStatement()
        {
            bool success = courtroomFlow.MoveNextStatement();

            if (success)
            {
                CurrentStatementChanged?.Invoke(
                    courtroomFlow.CurrentStatement);
            }

            return success;
        }

        public bool MovePreviousStatement()
        {
            return courtroomFlow.MovePreviousStatement();
        }

        private EvaluationResult Execute(
            EvaluationType type,
            EvidenceData evidence = null)
        {
            EvaluationResult result =
                evaluationSystem.Evaluate(type, evidence);

            effectProcessor.Apply(result);

            EvaluationCompleted?.Invoke(result);

            return result;
        }
    }
}
