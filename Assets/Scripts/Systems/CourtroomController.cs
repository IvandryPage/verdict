using System;
using Verdict.Data.Cases;
using Verdict.Data.Dialogue;
using Verdict.Data.Evidence;
using Verdict.Runtime;
using Verdict.Runtime.Dialogue;

namespace Verdict.Systems
{
    /// <summary>
    /// Coordinates gameplay flow and dialogue playback.
    /// Does not know what dialogue *contains* - only that a statement may
    /// have dialogue bound to it, and that dialogue must finish before
    /// gameplay is considered active for that statement.
    /// </summary>
    public sealed class CourtroomController
    {
        private readonly CaseSessionManager caseSessionManager;

        private bool isInteractionLocked;

        private CaseSession Session =>
            caseSessionManager.CurrentSession;

        private DialogueCoordinator DialogueCoordinator =>
            Session?.DialogueCoordinator;

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

        /// <summary>
        /// True once the current statement's dialogue (if any) has reached
        /// its StatementMarker / finished, and gameplay actions (Press,
        /// Question, PresentEvidence, RemainSilent) are allowed.
        /// </summary>
        public bool CanInteract =>
            HasActiveCase && !isInteractionLocked;

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

            DialogueCoordinator.DialogueFinished += HandleDialogueFinished;
            DialogueCoordinator.StatementReached += HandleStatementReached;
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
            if (!CanInteract)
            {
                throw new InvalidOperationException(
                    "Cannot interact while dialogue is playing.");
            }

            EvaluationResult result =
                Session.EvaluationSystem.Evaluate(
                    evaluationType,
                    evidence);

            CourtStateEffectProcessingResult effectResult =
                Session.EffectProcessor.Apply(result);

            bool jumped = HandleFlowIntents(effectResult);

            if (!jumped)
            {
                ResumeDialogueAfterEvaluation();
            }

            EvaluationCompleted?.Invoke(result);

            return result;
        }

        /// <summary>
        /// If the current statement's dialogue is paused at its
        /// StatementMarker and evaluation didn't jump elsewhere, resume it
        /// so any reaction/consequence lines after the marker get played.
        /// </summary>
        private void ResumeDialogueAfterEvaluation()
        {
            if (!DialogueCoordinator.CanResume)
                return;

            isInteractionLocked = true;

            DialogueCoordinator.Resume();
        }

        private bool HandleFlowIntents(
            CourtStateEffectProcessingResult effectResult)
        {
            if (effectResult == null)
            {
                return false;
            }

            foreach (CourtStateEffectIntent intent in effectResult.Intents)
            {
                switch (intent.Effect)
                {
                    case CourtStateEffect.JumpStatement:

                        Session.Flow.GoToStatement(
                            intent.TargetId);

                        RefreshCurrentStatement();
                        return true;

                    case CourtStateEffect.JumpTestimony:

                        Session.Flow.GoToTestimony(
                            intent.TargetId);

                        RefreshCurrentStatement();
                        return true;

                    case CourtStateEffect.JumpWitness:

                        Session.Flow.GoToWitness(
                            intent.TargetId);

                        RefreshCurrentStatement();
                        return true;
                }
            }

            return false;
        }

        private void RefreshCurrentStatement()
        {

            CurrentStatementChanged?.Invoke(
                CurrentStatement);

            DialogueData dialogue = CurrentStatement?.Dialogue;

            if (dialogue != null)
            {
                isInteractionLocked = true;

                DialogueCoordinator.Play(dialogue);
            }
            else
            {
                isInteractionLocked = false;
            }
        }

        private void HandleStatementMarkerReached()
        {
            isInteractionLocked = false;
        }

        private void HandleDialogueFinished()
        {
            isInteractionLocked = false;
        }

        private void HandleStatementReached()
        {
            isInteractionLocked = false;
        }
    }
}
