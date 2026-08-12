using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Data.Characters;
using Verdict.Data.Evidence;
using Verdict.Data.Narrative;
using Verdict.Runtime;

namespace Verdict.Systems
{
    /// <summary>
    /// Coordinates courtroom gameplay, narrative playback,
    /// player actions, evaluation and flow.
    ///
    /// CourtroomController does not decide whether an action is
    /// correct. ResolverEngine owns that responsibility.
    /// </summary>
    public sealed class CourtroomController
    {
        private readonly CaseSessionManager caseSessionManager;

        private NarrativeCoordinator subscribedCoordinator;

        private CourtroomState courtroomState =
            CourtroomState.None;

        private CourtroomState stateBeforePause;

        private readonly List<EvidenceData> selectedEvidence = new();

        private CaseSession Session =>
            caseSessionManager.CurrentSession;

        private NarrativeCoordinator Narrative =>
            Session?.NarrativeCoordinator;

        // Current runtime data

        public StatementRuntime CurrentStatement =>
            Session?.Flow.CurrentStatement;

        public TestimonyRuntime CurrentTestimony =>
            Session?.Flow.CurrentTestimony;

        public WitnessRuntime CurrentWitness =>
            Session?.Flow.CurrentWitness;

        public CourtStateRuntime CourtState =>
            Session?.Runtime.CourtState;

        // State

        public CourtroomState CourtroomState =>
            courtroomState;

        public IReadOnlyList<EvidenceData> SelectedEvidence =>
            selectedEvidence;

        public bool HasActiveCase =>
            caseSessionManager.HasActiveSession;

        public bool CanInteract =>
            HasActiveCase &&
            courtroomState == CourtroomState.Statement &&
            (Narrative == null ||
             Narrative.IsWaitingForStatement);

        public bool CanPresentSelectedEvidence =>
            selectedEvidence.Count > 0;

        public bool IsWaitingForChoice =>
            Narrative?.IsWaitingForChoice ?? false;

        // Narrative data

        public NarrativeDialogueEntryData CurrentNarrativeEntry =>
            Narrative?.CurrentEntry;

        public NarrativeLineData CurrentNarrativeLine =>
            Narrative?.CurrentLine;

        public ChoiceNodeData CurrentChoice =>
            Narrative?.CurrentChoice;

        // Flow

        public bool CanMoveNextStatement =>
            Session?.Flow.CanMoveNextStatement ?? false;

        public bool CanMovePreviousStatement =>
            Session?.Flow.CanMovePreviousStatement ?? false;

        public bool IsLastStatement =>
            Session?.Flow.IsLastStatement ?? true;

        // Events

        public event Action CaseStarted;
        public event Action CaseRestarted;
        public event Action CaseFinished;

        public event Action<StatementRuntime>
            CurrentStatementChanged;

        public event Action<ResolverResult>
            ArgumentResolved;

        public event Action<EndingData>
            EndingTriggered;

        public event Action<NarrativeDialogueEntryData>
            NarrativeEntryChanged;

        public event Action<ChoiceNodeData>
            ChoiceRequested;

        public event Action<NarrativeEventData>
            PresentationEventTriggered;

        public event Action<GameplayNodeData>
            GameplayEventTriggered;

        public event Action<CourtroomState>
            CourtroomStateChanged;

        public event Action<IReadOnlyList<EvidenceData>>
            EvidenceSelectionChanged;

        private CourtroomState previousStateBeforeEvidenceSelection;

        // Constructor

        public CourtroomController(
            CaseSessionManager caseSessionManager)
        {
            this.caseSessionManager =
                caseSessionManager ??
                throw new ArgumentNullException(
                    nameof(caseSessionManager));
        }

        // CASE

        public void BeginCase()
        {
            if (!HasActiveCase)
            {
                throw new InvalidOperationException(
                    "No active case.");
            }

            Session.Flow.Reset();

            ClearSelectedEvidence();

            SetCourtroomState(
                CourtroomState.None);

            EnsureNarrativeSubscribed();

            CurrentStatementChanged?.Invoke(
                CurrentStatement);

            Narrative?.TryPlay(
                Session.Runtime.Data.Narrative);

            CaseStarted?.Invoke();
        }

        public void RestartCase()
        {
            caseSessionManager.RestartCase();

            Session.Flow.Reset();

            ClearSelectedEvidence();

            SetCourtroomState(
                CourtroomState.None);

            EnsureNarrativeSubscribed();

            CurrentStatementChanged?.Invoke(
                CurrentStatement);

            Narrative?.TryPlay(
                Session.Runtime.Data.Narrative);

            CaseRestarted?.Invoke();
        }

        public void EndCase()
        {
            SetCourtroomState(
                CourtroomState.None);

            CaseFinished?.Invoke();
        }

        // PLAYER ACTIONS

        #region Press

        public void BeginPress()
        {
            EnsureStatementInteraction();

            SetCourtroomState(
                CourtroomState.Pressing);
        }

        public ResolverResult ResolvePress()
        {
            EnsureState(
                CourtroomState.Pressing);

            return ExecuteArgument(
                PlayerArgumentData.Press(
                    CurrentStatement?.Data));
        }

        #endregion


        #region Question

        public void BeginQuestion()
        {
            EnsureStatementInteraction();

            SetCourtroomState(
                CourtroomState.Questioning);
        }

        public ResolverResult ResolveQuestion()
        {
            EnsureState(
                CourtroomState.Questioning);

            return ExecuteArgument(
                PlayerArgumentData.Question(
                    CurrentStatement?.Data));
        }

        #endregion


        #region Present Evidence

        public void BeginPresentEvidence()
        {
            EnsureStatementInteraction();

            previousStateBeforeEvidenceSelection =
                    courtroomState;

            SetCourtroomState(
                CourtroomState.EvidenceSelection);
        }

        public void CancelEvidenceSelection()
        {
            EnsureState(
                CourtroomState.EvidenceSelection);

            SetCourtroomState(
                previousStateBeforeEvidenceSelection);
        }

        public ResolverResult ResolvePresentEvidence(
            IEnumerable<EvidenceData> evidence)
        {
            EnsureState(
                CourtroomState.EvidenceSelection);

            if (evidence == null)
            {
                throw new ArgumentNullException(
                    nameof(evidence));
            }

            List<EvidenceData> evidenceList =
                evidence
                    .Where(e => e != null)
                    .Distinct()
                    .ToList();

            if (evidenceList.Count == 0)
            {
                throw new ArgumentException(
                    "At least one evidence item must be presented.",
                    nameof(evidence));
            }

            return ExecuteArgument(
                PlayerArgumentData.PresentEvidence(
                    evidenceList,
                    CurrentStatement?.Data));
        }

        #endregion


        #region Remain Silent

        public ResolverResult RemainSilent()
        {
            EnsureStatementInteraction();

            return ExecuteArgument(
                PlayerArgumentData.RemainSilent(
                    CurrentStatement?.Data));
        }

        #endregion


        #region Bluff

        public void BeginBluff()
        {
            EnsureStatementInteraction();

            SetCourtroomState(
                CourtroomState.Statement);
        }

        public ResolverResult ResolveBluff()
        {
            EnsureState(
                CourtroomState.Statement);

            return ExecuteArgument(
                new PlayerArgumentData(
                    PlayerAction.Bluff,
                    selectedStatement:
                        CurrentStatement?.Data));
        }

        #endregion


        #region Threaten

        public void BeginThreaten()
        {
            EnsureStatementInteraction();

            SetCourtroomState(
                CourtroomState.Statement);
        }

        public ResolverResult ResolveThreaten()
        {
            EnsureState(
                CourtroomState.Statement);

            return ExecuteArgument(
                new PlayerArgumentData(
                    PlayerAction.Threaten,
                    selectedStatement:
                        CurrentStatement?.Data));
        }

        #endregion


        #region Object

        public void BeginObject()
        {
            EnsureStatementInteraction();

            SetCourtroomState(
                CourtroomState.Statement);
        }

        public ResolverResult ResolveObject()
        {
            EnsureState(
                CourtroomState.Statement);

            return ExecuteArgument(
                new PlayerArgumentData(
                    PlayerAction.Object,
                    selectedStatement:
                        CurrentStatement?.Data));
        }

        #endregion


        #region Interrupt

        public void BeginInterrupt()
        {
            EnsureStatementInteraction();

            SetCourtroomState(
                CourtroomState.Statement);
        }

        public ResolverResult ResolveInterrupt()
        {
            EnsureState(
                CourtroomState.Statement);

            return ExecuteArgument(
                new PlayerArgumentData(
                    PlayerAction.Interrupt,
                    selectedStatement:
                        CurrentStatement?.Data));
        }

        #endregion


        #region Compare Evidence

        public void BeginCompareEvidence()
        {
            EnsureStatementInteraction();

            SetCourtroomState(
                CourtroomState.EvidenceInspection);
        }

        public ResolverResult ResolveCompareEvidence(
            IReadOnlyList<EvidenceData> evidence)
        {
            EnsureState(
                CourtroomState.EvidenceInspection);

            return ExecuteArgument(
                PlayerArgumentData.CompareEvidence(
                    evidence,
                    CurrentStatement?.Data));
        }

        #endregion

        // EVIDENCE

        public void BeginEvidenceSelection()
        {
            EnsureStatementInteraction();

            ClearSelectedEvidence();

            SetCourtroomState(
                CourtroomState.EvidenceSelection);
        }

        public void ToggleEvidenceSelection(
            EvidenceData evidence)
        {
            EnsureState(
                CourtroomState.EvidenceSelection);

            if (evidence == null)
            {
                throw new ArgumentNullException(
                    nameof(evidence));
            }

            if (selectedEvidence.Contains(evidence))
            {
                selectedEvidence.Remove(evidence);
            }
            else
            {
                selectedEvidence.Add(evidence);
            }

            EvidenceSelectionChanged?.Invoke(
                SelectedEvidence);
        }

        public void ReviewSelectedEvidence()
        {
            EnsureState(
                CourtroomState.EvidenceSelection);

            if (selectedEvidence.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one evidence item must be selected.");
            }

            SetCourtroomState(
                CourtroomState.EvidenceInspection);
        }

        public ResolverResult PresentSelectedEvidence()
        {
            EnsureState(
                CourtroomState.EvidenceInspection);

            if (selectedEvidence.Count == 0)
            {
                throw new InvalidOperationException(
                    "No evidence selected.");
            }

            return ExecuteArgument(
                PlayerArgumentData.PresentEvidence(
                    selectedEvidence,
                    CurrentStatement?.Data));
        }

        // RESULT

        public bool ContinueFromResult()
        {
            EnsureState(
                CourtroomState.Result);

            ClearSelectedEvidence();

            return ResumeNarrative();
        }

        // NARRATIVE

        public bool SelectChoice(int choiceIndex)
        {
            return Narrative?.SelectChoice(
                choiceIndex) ?? false;
        }

        public bool ResumeNarrative()
        {
            return Narrative?.TryResume() ?? false;
        }

        // STATEMENT NAVIGATION

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

        // ARGUMENT EXECUTION

        private ResolverResult ExecuteArgument(
            PlayerArgumentData argument)
        {
            if (!HasActiveCase)
            {
                throw new InvalidOperationException(
                    "No active case.");
            }

            SetCourtroomState(
                CourtroomState.Evaluating);

            // Resolver decides whether the action succeeds.
            ResolverResult result =
                Session.ResolverEngine.Resolve(
                    argument);

            // Successful/failed consequences are handled here.
            CourtStateEffectProcessingResult effectResult =
                Session.EffectProcessor.Apply(
                    result);

            bool jumped =
                HandleFlowIntents(
                    effectResult);

            ArgumentResolved?.Invoke(
                result);

            if (jumped)
            {
                return result;
            }

            SetCourtroomState(
                CourtroomState.Statement);

            return result;
        }

        // FLOW INTENTS

        private bool HandleFlowIntents(
    CourtStateEffectProcessingResult effectResult)
        {
            if (effectResult == null)
            {
                return false;
            }

            foreach (CourtStateEffectIntent intent
                in effectResult.Intents)
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

                    case CourtStateEffect.TriggerEnding:

                        HandleEndingReached(
                            intent.TargetId);

                        return true;
                }
            }

            return false;
        }
        // NARRATIVE EVENTS

        private void EnsureNarrativeSubscribed()
        {
            NarrativeCoordinator coordinator =
                Narrative;

            if (coordinator == null ||
                ReferenceEquals(
                    coordinator,
                    subscribedCoordinator))
            {
                return;
            }

            if (subscribedCoordinator != null)
            {
                subscribedCoordinator.StatementReached -=
                    HandleStatementReached;

                subscribedCoordinator.EndingReached -=
                    HandleEndingReached;

                subscribedCoordinator.EventTriggered -=
                    HandlePresentationEvent;

                subscribedCoordinator.GameplayNodeReached -=
                    HandleGameplayNodeReached;

                subscribedCoordinator.EntryChanged -=
                    HandleEntryChanged;

                subscribedCoordinator.ChoiceRequested -=
                    HandleChoiceRequested;
            }

            coordinator.StatementReached +=
                HandleStatementReached;

            coordinator.EndingReached +=
                HandleEndingReached;

            coordinator.EventTriggered +=
                HandlePresentationEvent;

            coordinator.GameplayNodeReached +=
                HandleGameplayNodeReached;

            coordinator.EntryChanged +=
                HandleEntryChanged;

            coordinator.ChoiceRequested +=
                HandleChoiceRequested;

            subscribedCoordinator =
                coordinator;
        }

        private void HandleStatementReached(
            string statementId)
        {
            if (!string.IsNullOrWhiteSpace(
                statementId))
            {
                Session.Flow.TryMoveToStatement(
                    statementId);
            }

            ClearSelectedEvidence();

            SetCourtroomState(
                CourtroomState.Statement);

            CurrentStatementChanged?.Invoke(
                CurrentStatement);
        }

        private void HandleEndingReached(
            string endingId)
        {
            Debug.Log(
                $"[Ending] Reached ending ID: {endingId}");

            if (!string.IsNullOrWhiteSpace(endingId))
            {
                EndingData ending =
                    Session.Runtime.Data.Endings
                        .FirstOrDefault(e => e.Id == endingId);

                Debug.Log(
                    $"[Ending] Found EndingData: {ending?.Title}");

                if (ending != null)
                {
                    Debug.Log(
                        "[Ending] Invoking EndingTriggered");

                    SetCourtroomState(
                        CourtroomState.Ending);

                    EndingTriggered?.Invoke(ending);
                }
            }
        }

        private void HandlePresentationEvent(
            NarrativeEventData eventData)
        {
            PresentationEventTriggered?.Invoke(
                eventData);
        }

        private void HandleGameplayNodeReached(
            GameplayNodeData node)
        {
            GameplayEventTriggered?.Invoke(
                node);
        }

        private void HandleEntryChanged(
            NarrativeDialogueEntryData entry)
        {
            NarrativeEntryChanged?.Invoke(
                entry);
        }

        private void HandleChoiceRequested(
            ChoiceNodeData choice)
        {
            ChoiceRequested?.Invoke(
                choice);
        }

        // STATEMENT / NARRATIVE SYNC

        private void SyncNarrativeToCurrentStatement()
        {
            ClearSelectedEvidence();

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
                Narrative?.JumpToNode(
                    nodeId);
            }
        }

        // STATE

        private void SetCourtroomState(
            CourtroomState newState)
        {
            if (courtroomState == newState)
            {
                return;
            }

            courtroomState =
                newState;

            CourtroomStateChanged?.Invoke(
                courtroomState);
        }

        private void EnsureStatementInteraction()
        {
            if (!HasActiveCase)
            {
                throw new InvalidOperationException(
                    "No active case.");
            }

            if (!CanInteract)
            {
                throw new InvalidOperationException(
                    "Player cannot interact right now.");
            }
        }

        private void EnsureState(
            CourtroomState expectedState)
        {
            if (courtroomState != expectedState)
            {
                throw new InvalidOperationException(
                    $"Expected courtroom state " +
                    $"{expectedState}, but current state is " +
                    $"{courtroomState}.");
            }
        }

        private void ClearSelectedEvidence()
        {
            if (selectedEvidence.Count == 0)
            {
                return;
            }

            selectedEvidence.Clear();

            EvidenceSelectionChanged?.Invoke(
                SelectedEvidence);
        }

        public CharacterData GetCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            return Session?.Runtime.Data.Characters
                .FirstOrDefault(c => c.Id == characterId);
        }

        public IReadOnlyList<EvidenceData> GetAvailableEvidence()
        {
            if (!HasActiveCase)
            {
                return Array.Empty<EvidenceData>();
            }

            return Session.Runtime.Evidence
                .Where(evidence =>
                    evidence != null &&
                    evidence.Data != null &&
                    evidence.IsUnlocked &&
                    IsEvidencePresentable(evidence.Data))
                .Select(evidence =>
                    evidence.Data)
                .ToList();
        }

        private bool IsEvidencePresentable(
            EvidenceData evidence)
        {
            if (evidence == null)
            {
                return false;
            }

            EvidenceEntryData entry =
                Session.Runtime.Data.Evidence
                    .FirstOrDefault(
                        e => e.Evidence == evidence);

            return entry != null &&
                   entry.CanPresent;
        }

        public void Pause()
        {
            if (courtroomState == CourtroomState.Paused ||
                courtroomState == CourtroomState.Ending)
            {
                return;
            }

            stateBeforePause = courtroomState;

            SetCourtroomState(
                CourtroomState.Paused);
        }

        public void Resume()
        {
            if (courtroomState != CourtroomState.Paused)
            {
                return;
            }

            SetCourtroomState(
                stateBeforePause);
        }
    }
}
