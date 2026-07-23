using System;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;
using Verdict.Data.Narrative;
using Verdict.Runtime;

namespace Verdict.Systems
{
    /// <summary>
    /// Coordinates gameplay flow and narrative playback.
    /// No longer owns narrative logic - it only reacts to
    /// NarrativeCoordinator events (a statement was reached, the graph
    /// ended) and tells it when to resume or jump. Controller only knows
    /// Gameplay, Flow, Evaluation and CourtState.
    /// </summary>
    public sealed class CourtroomController
    {
        private readonly CaseSessionManager caseSessionManager;

        private NarrativeCoordinator subscribedCoordinator;

        private CaseSession Session =>
            caseSessionManager.CurrentSession;

        private NarrativeCoordinator Narrative =>
            Session?.NarrativeCoordinator;

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
        /// True once the narrative graph has paused on the current
        /// statement's StatementNodeData (or has no narrative at all),
        /// and gameplay actions (Press, Question, PresentEvidence,
        /// RemainSilent) are allowed.
        /// </summary>
        public bool CanInteract =>
            HasActiveCase &&
            (Narrative == null ||
            Narrative.IsWaitingForStatement ||
            !Narrative.IsPlaying);

        public bool IsWaitingForChoice =>
            Narrative?.IsWaitingForChoice ?? false;

        public NarrativeDialogueEntryData CurrentNarrativeEntry =>
            Narrative?.CurrentEntry;

        public NarrativeLineData CurrentNarrativeLine =>
            Narrative?.CurrentLine;

        public ChoiceNodeData CurrentChoice =>
            Narrative?.CurrentChoice;

        public event Action CaseStarted;
        public event Action CaseRestarted;
        public event Action CaseFinished;

        /// <summary>
        /// A Dialogue node's Event entry fired (camera/music/sound cue -
        /// pure presentation, nothing gameplay-related). Wire your AV
        /// system to this.
        /// </summary>
        public event Action<NarrativeEventData> PresentationEventTriggered;

        /// <summary>
        /// The graph passed through a Gameplay node (unlock a feature,
        /// start a minigame, mark a checkpoint - whatever your project
        /// defines). Wire your gameplay systems to this.
        /// </summary>
        public event Action<GameplayNodeData> GameplayEventTriggered;

        public event Action<StatementRuntime> CurrentStatementChanged;
        public event Action<ResolverResult> ArgumentResolved;
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

            EnsureNarrativeSubscribed();

            CurrentStatementChanged?.Invoke(CurrentStatement);

            Narrative.TryPlay(Session.Runtime.Data.Narrative);

            CaseStarted?.Invoke();
        }

        public void RestartCase()
        {
            caseSessionManager.RestartCase();

            Session.Flow.Reset();

            EnsureNarrativeSubscribed();

            CurrentStatementChanged?.Invoke(CurrentStatement);

            Narrative.TryPlay(Session.Runtime.Data.Narrative);

            CaseRestarted?.Invoke();
        }

        public void EndCase()
        {
            CaseFinished?.Invoke();
        }

        public ResolverResult PresentEvidence(
            EvidenceData evidence)
        {
            return Execute(
                PlayerArgumentData.PresentEvidence(
                    evidence,
                    CurrentStatement?.Data));
        }

        public ResolverResult Press()
        {
            return Execute(
                PlayerArgumentData.Press(
                    CurrentStatement?.Data));
        }

        public ResolverResult Question()
        {
            return Execute(
                PlayerArgumentData.Question(
                    CurrentStatement?.Data));
        }

        public ResolverResult RemainSilent()
        {
            return Execute(
                PlayerArgumentData.RemainSilent(
                    CurrentStatement?.Data));
        }

        public bool SelectChoice(int choiceIndex)
        {
            return Narrative?.SelectChoice(choiceIndex) ?? false;
        }

        public bool Continue()
        {
            bool success =
                Session.Flow.MoveNext();

            if (success)
            {
                SyncNarrativeToCurrentStatement();
            }

            return success;
        }

        public bool MovePreviousStatement()
        {
            bool success =
                Session.Flow.MovePreviousStatement();

            if (success)
            {
                SyncNarrativeToCurrentStatement();
            }

            return success;
        }

        private ResolverResult Execute(
            PlayerArgumentData argument)
        {
            if (!CanInteract)
            {
                throw new InvalidOperationException(
                    "Cannot interact while narrative is playing.");
            }

            ResolverResult result =
                Session.ResolverEngine.Resolve(argument);

            CourtStateEffectProcessingResult effectResult =
                Session.EffectProcessor.Apply(result);

            bool jumped = HandleFlowIntents(effectResult);

            if (!jumped)
            {
                ResumeNarrativeAfterEvaluation();
            }

            ArgumentResolved?.Invoke(result);

            return result;
        }

        /// <summary>
        /// If the graph is paused at the current statement's node and
        /// evaluation didn't jump elsewhere, resume it so any reaction/
        /// consequence content after the statement node gets played.
        /// </summary>
        private void ResumeNarrativeAfterEvaluation()
        {
            Narrative?.TryResume();
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

                        SyncNarrativeToCurrentStatement();
                        return true;

                    case CourtStateEffect.JumpTestimony:

                        Session.Flow.GoToTestimony(
                            intent.TargetId);

                        SyncNarrativeToCurrentStatement();
                        return true;

                    case CourtStateEffect.JumpWitness:

                        Session.Flow.GoToWitness(
                            intent.TargetId);

                        SyncNarrativeToCurrentStatement();
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called whenever Flow moves to a different statement on its own
        /// (Continue/MovePreviousStatement/effect-driven jumps). Notifies
        /// listeners and, if the narrative graph has a node bound to this
        /// statement, jumps the graph there too so presentation stays in
        /// sync with gameplay navigation.
        /// </summary>
        private void SyncNarrativeToCurrentStatement()
        {
            CurrentStatementChanged?.Invoke(
                CurrentStatement);

            if (CurrentStatement == null)
            {
                return;
            }

            if (Session.Runtime.TryGetNodeIdForStatement(
                CurrentStatement.Data.Id,
                out string nodeId))
            {
                Narrative.JumpToNode(nodeId);
            }
        }

        private void EnsureNarrativeSubscribed()
        {
            NarrativeCoordinator coordinator = Narrative;

            if (coordinator == null ||
                ReferenceEquals(coordinator, subscribedCoordinator))
            {
                return;
            }

            if (subscribedCoordinator != null)
            {
                subscribedCoordinator.StatementReached -= HandleStatementReached;
                subscribedCoordinator.EndingReached -= HandleEndingReached;
                subscribedCoordinator.EventTriggered -= HandlePresentationEvent;
                subscribedCoordinator.GameplayNodeReached -= HandleGameplayNodeReached;
            }

            coordinator.StatementReached += HandleStatementReached;
            coordinator.EndingReached += HandleEndingReached;
            coordinator.EventTriggered += HandlePresentationEvent;
            coordinator.GameplayNodeReached += HandleGameplayNodeReached;

            subscribedCoordinator = coordinator;
        }

        private void HandlePresentationEvent(NarrativeEventData eventData)
        {
            PresentationEventTriggered?.Invoke(eventData);
        }

        private void HandleGameplayNodeReached(GameplayNodeData node)
        {
            GameplayEventTriggered?.Invoke(node);
        }

        /// <summary>
        /// The graph reached a StatementNodeData on its own (natural
        /// progression, not a Flow-driven jump). Sync Flow to match so
        /// ResolverEngine/EffectProcessor read the right statement.
        /// </summary>
        private void HandleStatementReached(string statementId)
        {
            if (!string.IsNullOrWhiteSpace(statementId))
            {
                Session.Flow.TryMoveToStatement(statementId);
            }

            CurrentStatementChanged?.Invoke(CurrentStatement);
        }

        private void HandleEndingReached(string endingId)
        {
            if (!string.IsNullOrWhiteSpace(endingId))
            {
                EndingData ending =
                    Session.Runtime.Data.Endings
                        .FirstOrDefault(e => e.Id == endingId);

                if (ending != null)
                {
                    EndingTriggered?.Invoke(ending);
                }
            }

            EndCase();
        }
    }
}
