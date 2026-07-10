using System;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CourtroomController
    {
        private readonly CaseSessionManager caseSessionManager;
        private readonly EvaluationSystem evaluationSystem;
        private readonly CourtStateEffectProcessor effectProcessor;

        private CourtroomFlow courtroomFlow;

        public StatementRuntime CurrentStatement =>
                    courtroomFlow.CurrentStatement;

        public TestimonyRuntime CurrentTestimony =>
            courtroomFlow.CurrentTestimony;

        public WitnessRuntime CurrentWitness =>
            courtroomFlow.CurrentWitness;

        public CourtStateRuntime CourtState =>
            caseSessionManager.CurrentCase?.CourtState;

        public bool HasActiveCase =>
            caseSessionManager.HasActiveCase;

        public bool CanMoveNextStatement =>
            courtroomFlow.CanMoveNextStatement;

        public bool CanMovePreviousStatement =>
            courtroomFlow.CanMovePreviousStatement;

        public bool IsLastStatement =>
            courtroomFlow.IsLastStatement;

        public event Action CaseStarted;
        public event Action CaseRestarted;
        public event Action CaseFinished;

        public event Action<StatementRuntime> CurrentStatementChanged;
        public event Action<EvaluationResult> EvaluationCompleted;
        public event Action<EndingData> EndingTriggered;


        public CourtroomController(
            CaseSessionManager caseSessionManager,
            CourtroomFlow courtroomFlow,
            EvaluationSystem evaluationSystem,
            CourtStateEffectProcessor effectProcessor)
        {
            this.caseSessionManager =
                caseSessionManager ??
                throw new ArgumentNullException(nameof(caseSessionManager));

            this.courtroomFlow =
                courtroomFlow ??
                throw new ArgumentNullException(nameof(courtroomFlow));

            this.evaluationSystem =
                evaluationSystem ??
                throw new ArgumentNullException(nameof(evaluationSystem));

            this.effectProcessor =
                effectProcessor ??
                throw new ArgumentNullException(nameof(effectProcessor));
        }

        public void BeginCase()
        {
            if (!HasActiveCase)
            {
                throw new InvalidOperationException(
                    "No active case.");
            }

            courtroomFlow =
                new CourtroomFlow(
                    caseSessionManager.CurrentCase);

            RefreshCurrentStatement();

            CaseStarted?.Invoke();
        }

        public void RestartCase()
        {
            caseSessionManager.RestartCase();

            courtroomFlow =
                new CourtroomFlow(
                    caseSessionManager.CurrentCase);

            RefreshCurrentStatement();

            CaseRestarted?.Invoke();
        }

        public void EndCase()
        {
            CaseFinished?.Invoke();
        }

        public EvaluationResult PresentEvidence(
            EvidenceData evidence)
        {
            return Execute(
                Data.Cases.EvaluationType.PresentEvidence,
                evidence);
        }

        public EvaluationResult Press()
        {
            return Execute(
                Data.Cases.EvaluationType.Press);
        }

        public EvaluationResult Question()
        {
            return Execute(
                Data.Cases.EvaluationType.Question);
        }

        public EvaluationResult RemainSilent()
        {
            return Execute(
                Data.Cases.EvaluationType.RemainSilent);
        }

        public bool Continue()
        {
            bool success = courtroomFlow.MoveNext();

            if (success)
            {
                RefreshCurrentStatement();
            }

            return success;
        }

        public bool MovePreviousStatement()
        {
            bool success =
                courtroomFlow.MovePreviousStatement();

            if (success)
            {
                RefreshCurrentStatement();
            }

            return success;
        }

        private EvaluationResult Execute(
            Data.Cases.EvaluationType evaluationType,
            EvidenceData evidence = null)
        {
            EvaluationResult result =
                evaluationSystem.Evaluate(
                    evaluationType,
                    evidence);

            CourtStateEffectProcessingResult effectResult =
                effectProcessor.Apply(result);

            HandleFlowIntents(effectResult);

            EvaluationCompleted?.Invoke(result);

            return result;
        }

        private void HandleFlowIntents(CourtStateEffectProcessingResult effectResult)
        {
            if (effectResult == null)
            {
                return;
            }

            foreach (CourtStateEffectIntent intent in effectResult.Intents)
            {
                switch (intent.Effect)
                {
                    case CourtStateEffect.JumpStatement:
                        courtroomFlow.GoToStatement(intent.TargetId);
                        RefreshCurrentStatement();
                        return;

                    case CourtStateEffect.JumpTestimony:
                        courtroomFlow.GoToTestimony(intent.TargetId);
                        RefreshCurrentStatement();
                        return;

                    case CourtStateEffect.JumpWitness:
                        courtroomFlow.GoToWitness(intent.TargetId);
                        RefreshCurrentStatement();
                        return;
                    case CourtStateEffect.TriggerEnding:
                        if (caseSessionManager.CurrentCase?.Data != null)
                        {
                            EndingData ending = caseSessionManager.CurrentCase.Data.Endings
                                .FirstOrDefault(e => e.Id == intent.TargetId);
                            if (ending != null)
                            {
                                EndingTriggered?.Invoke(ending);
                            }
                        }
                        return;
                }
            }
        }

        private void RefreshCurrentStatement()
        {
            CurrentStatementChanged?.Invoke(
                CurrentStatement);
        }
    }
}
