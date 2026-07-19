using System;
using Verdict.Data.Narrative;
using Verdict.Runtime.Narrative;

namespace Verdict.Systems
{
    /// <summary>
    /// Plays, resumes and stops the case's narrative graph, and tracks
    /// playback state. This is the only narrative-facing thing
    /// CourtroomController talks to.
    /// </summary>
    public sealed class NarrativeCoordinator
    {
        private readonly NarrativeRunner runner;

        public NarrativeCoordinator(NarrativeRunner runner)
        {
            this.runner = runner
                ?? throw new ArgumentNullException(nameof(runner));

            runner.NarrativeStarted += HandleNarrativeStarted;
            runner.NarrativeFinished += HandleNarrativeFinished;
            runner.StatementReached += HandleStatementReached;
            runner.ChoiceRequested += HandleChoiceRequested;
        }

        public NarrativeState State { get; private set; }
            = NarrativeState.Idle;

        public bool IsPlaying =>
            State == NarrativeState.Playing;

        public bool IsWaitingForStatement =>
            State == NarrativeState.WaitingForStatement;

        public bool IsWaitingForChoice =>
            State == NarrativeState.WaitingForChoice;

        public bool CanResume =>
            State == NarrativeState.WaitingForStatement;

        public string CurrentStatementId { get; private set; }

        public ChoiceNodeData CurrentChoice { get; private set; }

        public NarrativeDialogueEntryData CurrentEntry =>
            runner.Runtime?.CurrentNode is DialogueNodeData dialogueNode &&
            runner.Runtime.CurrentEntryIndex < dialogueNode.Entries.Count
                ? dialogueNode.Entries[runner.Runtime.CurrentEntryIndex]
                : null;

        public NarrativeLineData CurrentLine =>
            CurrentEntry?.Line;

        public bool HasActiveNarrative =>
            runner.Runtime != null;

        public event Action NarrativeStarted;

        public event Action NarrativeFinished;

        public event Action<string> StatementReached;

        public event Action<ChoiceNodeData> ChoiceRequested;

        public event Action<string> EndingReached
        {
            add => runner.EndingReached += value;
            remove => runner.EndingReached -= value;
        }

        public event Action<NarrativeDialogueEntryData> EntryChanged
        {
            add => runner.EntryChanged += value;
            remove => runner.EntryChanged -= value;
        }

        public event Action<NarrativeEventData> EventTriggered
        {
            add => runner.EventTriggered += value;
            remove => runner.EventTriggered -= value;
        }

        public void Play(NarrativeGraphData graph)
        {
            if (graph == null)
            {
                return;
            }

            State = NarrativeState.Playing;

            runner.Start(graph);
        }

        public bool TryPlay(NarrativeGraphData graph)
        {
            if (graph == null)
            {
                return false;
            }

            Play(graph);
            return true;
        }

        public void Resume()
        {
            if (!CanResume)
            {
                return;
            }

            State = NarrativeState.Playing;

            runner.Continue();
        }

        public bool TryResume()
        {
            if (!CanResume)
            {
                return false;
            }

            Resume();
            return true;
        }

        public bool SelectChoice(int choiceIndex)
        {
            if (State != NarrativeState.WaitingForChoice)
            {
                return false;
            }

            State = NarrativeState.Playing;

            return runner.SelectChoice(choiceIndex);
        }

        /// <summary>
        /// Forces the graph to a specific statement's node, if the
        /// narrative graph has one bound. Used when gameplay jumps to a
        /// statement outside the graph's normal linear order.
        /// </summary>
        public bool JumpToNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || !runner.IsRunning)
            {
                return false;
            }

            State = NarrativeState.Playing;

            runner.JumpTo(nodeId);
            return true;
        }

        public void Stop()
        {
            if (!runner.IsRunning)
            {
                return;
            }

            State = NarrativeState.Idle;

            runner.Stop();
        }

        public bool TryStop()
        {
            if (State == NarrativeState.Idle)
            {
                return false;
            }

            Stop();
            return true;
        }

        private void HandleNarrativeStarted(NarrativeRuntime runtime)
        {
            NarrativeStarted?.Invoke();
        }

        private void HandleNarrativeFinished(NarrativeRuntime runtime)
        {
            State = NarrativeState.Finished;

            NarrativeFinished?.Invoke();
        }

        private void HandleStatementReached(string statementId)
        {
            State = NarrativeState.WaitingForStatement;

            CurrentStatementId = statementId;

            StatementReached?.Invoke(statementId);
        }

        private void HandleChoiceRequested(ChoiceNodeData choice)
        {
            State = NarrativeState.WaitingForChoice;

            CurrentChoice = choice;

            ChoiceRequested?.Invoke(choice);
        }
    }
}
