using System;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CourtroomController
    {
        private readonly CaseSessionManager caseSessionManager;

        private CaseSession Session =>
            caseSessionManager.CurrentSession;

        public StatementRuntime CurrentStatement =>
            Session?.Flow.CurrentStatement;

        public TestimonyRuntime CurrentTestimony =>
            Session?.Flow.CurrentTestimony;

        public WitnessRuntime CurrentWitness =>
            Session?.Flow.CurrentWitness;

        public CourtStateRuntime CourtState =>
            Session?.Runtime.CourtState;

        public bool HasActiveCase =>
            caseSessionManager.HasActiveSession;

        public bool CanMoveNextStatement =>
            Session?.Flow.CanMoveNextStatement ?? false;

        public bool CanMovePreviousStatement =>
            Session?.Flow.CanMovePreviousStatement ?? false;

        public bool IsLastStatement =>
            Session?.Flow.IsLastStatement ?? true;

        public event Action CaseStarted;
        public event Action CaseRestarted;
        public event Action CaseFinished;

        public event Action<StatementRuntime> CurrentStatementChanged;
        public event Action<EvaluationResult> EvaluationCompleted;
        public event Action<EndingData> EndingTriggered;

        public CourtroomController(
            CaseSessionManager caseSessionManager)
        {
            this.caseSessionManager =
                caseSessionManager ??
                throw new ArgumentNullException(nameof(caseSessionManager));
        }

        public void BeginCase()
        {
            if (!HasActiveCase)
            {
                throw new InvalidOperationException(
                    "No active case.");
            }

            Session.Flow.Reset();

            RefreshCurrentStatement();

            CaseStarted?.Invoke();
        }

        public void RestartCase()
        {
            caseSessionManager.RestartCase();

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
                EvaluationType.PresentEvidence,
                evidence);
        }

        public EvaluationResult Press()
        {
            return Execute(
                EvaluationType.Press);
        }

        public EvaluationResult Question()
        {
            return Execute(
                EvaluationType.Question);
        }

        public EvaluationResult RemainSilent()
        {
            return Execute(
                EvaluationType.RemainSilent);
        }

        public bool Continue()
        {
            bool success =
                Session.Flow.MoveNext();

            if (success)
            {
                RefreshCurrentStatement();
            }

            return success;
        }

        public bool MovePreviousStatement()
        {
            bool success =
                Session.Flow.MovePreviousStatement();

            if (success)
            {
                RefreshCurrentStatement();
            }

            return success;
        }

        private EvaluationResult Execute(
            EvaluationType evaluationType,
            EvidenceData evidence = null)
        {
            EvaluationResult result =
                Session.EvaluationSystem.Evaluate(
                    evaluationType,
                    evidence);

            CourtStateEffectProcessingResult effectResult =
                Session.EffectProcessor.Apply(result);

            HandleFlowIntents(effectResult);

            EvaluationCompleted?.Invoke(result);

            return result;
        }

        private void HandleFlowIntents(
            CourtStateEffectProcessingResult effectResult)
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

                        Session.Flow.GoToStatement(
                            intent.TargetId);

                        RefreshCurrentStatement();
                        return;

                    case CourtStateEffect.JumpTestimony:

                        Session.Flow.GoToTestimony(
                            intent.TargetId);

                        RefreshCurrentStatement();
                        return;

                    case CourtStateEffect.JumpWitness:

                        Session.Flow.GoToWitness(
                            intent.TargetId);

                        RefreshCurrentStatement();
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
