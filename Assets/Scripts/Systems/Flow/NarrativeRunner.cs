using System;
using Verdict.Data.Narrative;
using Verdict.Runtime;
using Verdict.Runtime.Narrative;

namespace Verdict.Systems
{
    /// <summary>
    /// Executes narrative graph progression.
    /// Owns no UI. Owns no gameplay beyond reading CourtState/Claim
    /// resolution state for condition nodes. Only advances a
    /// NarrativeRuntime.
    /// </summary>
    public sealed class NarrativeRunner
    {
        private readonly CaseRuntime caseRuntime;

        public NarrativeRunner(CaseRuntime caseRuntime)
        {
            this.caseRuntime = caseRuntime
                ?? throw new ArgumentNullException(nameof(caseRuntime));
        }

        public event Action<NarrativeRuntime> NarrativeStarted;

        public event Action<NarrativeRuntime> NarrativeFinished;

        public event Action<NarrativeNodeData> NodeEntered;

        public event Action<NarrativeNodeData> NodeExited;

        /// <summary>
        /// Raised specifically when the graph reaches an EndNodeData,
        /// carrying its EndingId (may be null/empty).
        /// </summary>
        public event Action<string> EndingReached;

        public event Action<NarrativeDialogueEntryData> EntryChanged;

        public event Action<NarrativeEventData> EventTriggered;

        /// <summary>
        /// Raised when the graph reaches a StatementNodeData. Carries only
        /// the referenced StatementId - the runner does not know what a
        /// "statement" is beyond that string.
        /// </summary>
        public event Action<string> StatementReached;

        public event Action<ChoiceNodeData> ChoiceRequested;

        public event Action<ConditionNodeData, bool> ConditionEvaluated;

        /// <summary>
        /// Raised when the graph reaches a GameplayNodeData. Fire-and-continue -
        /// the runner does not pause for this on its own.
        /// </summary>
        public event Action<string> GameplayNodeReached;

        public NarrativeRuntime Runtime { get; private set; }

        public bool IsRunning =>
            Runtime != null &&
            !Runtime.IsFinished;

        public void Start(NarrativeGraphData graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            Runtime = new NarrativeRuntime(graph);

            NarrativeStarted?.Invoke(Runtime);

            ProcessCurrentNode();
        }

        /// <summary>
        /// Advances past the current pause point:
        /// - mid-dialogue: shows the next entry, or leaves the node when entries are exhausted
        /// - at a StatementNodeData: moves on to its NextNodeId (gameplay resolved)
        /// - at a GameplayNodeData: moves on to its NextNodeId
        /// Choice nodes are not advanced this way - use SelectChoice.
        /// </summary>
        public void Continue()
        {
            if (!IsRunning)
            {
                return;
            }

            switch (Runtime.CurrentNode)
            {
                case DialogueNodeData dialogueNode:

                    Runtime.CurrentEntryIndex++;

                    if (ProcessDialogueEntry(dialogueNode))
                    {
                        return;
                    }

                    AdvanceTo(dialogueNode.NextNodeId, dialogueNode);
                    ProcessCurrentNode();
                    return;

                case StatementNodeData statementNode:

                    AdvanceTo(statementNode.NextNodeId, statementNode);
                    ProcessCurrentNode();
                    return;

                case GameplayNodeData gameplayNode:

                    AdvanceTo(gameplayNode.NextNodeId, gameplayNode);
                    ProcessCurrentNode();
                    return;
            }
        }

        public bool SelectChoice(int choiceIndex)
        {
            if (!IsRunning || Runtime.CurrentNode is not ChoiceNodeData choiceNode)
            {
                return false;
            }

            if (choiceIndex < 0 || choiceIndex >= choiceNode.Choices.Count)
            {
                return false;
            }

            AdvanceTo(choiceNode.Choices[choiceIndex].NextNodeId, choiceNode);
            ProcessCurrentNode();
            return true;
        }

        /// <summary>
        /// Forces the graph cursor directly to a node id (used when
        /// gameplay jumps to a statement out of the graph's normal
        /// sequence) and continues processing from there.
        /// </summary>
        public void JumpTo(string nodeId)
        {
            if (Runtime == null)
            {
                return;
            }

            AdvanceTo(nodeId, Runtime.CurrentNode);
            ProcessCurrentNode();
        }

        public void Stop()
        {
            Runtime = null;
        }

        private void ProcessCurrentNode()
        {
            while (!Runtime.IsFinished)
            {
                NarrativeNodeData node = Runtime.CurrentNode;

                NodeEntered?.Invoke(node);

                switch (node)
                {
                    case DialogueNodeData dialogueNode:

                        if (ProcessDialogueEntry(dialogueNode))
                        {
                            return;
                        }

                        AdvanceTo(dialogueNode.NextNodeId, dialogueNode);
                        continue;

                    case StatementNodeData statementNode:

                        StatementReached?.Invoke(statementNode.StatementId);
                        return;

                    case ChoiceNodeData choiceNode:

                        ChoiceRequested?.Invoke(choiceNode);
                        return;

                    case ConditionNodeData conditionNode:
                        {
                            bool result = EvaluateCondition(conditionNode.Condition);

                            ConditionEvaluated?.Invoke(conditionNode, result);

                            AdvanceTo(
                                result ? conditionNode.TrueNodeId : conditionNode.FalseNodeId,
                                conditionNode);

                            continue;
                        }

                    case JumpNodeData jumpNode:

                        AdvanceTo(jumpNode.TargetNodeId, jumpNode);
                        continue;

                    case GameplayNodeData gameplayNode:

                        GameplayNodeReached?.Invoke(gameplayNode.GameplayEventId);

                        AdvanceTo(gameplayNode.NextNodeId, gameplayNode);
                        continue;

                    case EndNodeData endNode:

                        NodeExited?.Invoke(endNode);
                        Runtime.MoveTo(null);

                        EndingReached?.Invoke(endNode.EndingId);
                        NarrativeFinished?.Invoke(Runtime);
                        return;
                }
            }

            NarrativeFinished?.Invoke(Runtime);
        }

        /// <summary>
        /// True if paused on a Line entry (returns true), false if the
        /// node's entries are exhausted and it's time to move on.
        /// Events within the sequence fire and are skipped over
        /// automatically - they never pause.
        /// </summary>
        private bool ProcessDialogueEntry(DialogueNodeData node)
        {
            while (Runtime.CurrentEntryIndex < node.Entries.Count)
            {
                NarrativeDialogueEntryData entry =
                    node.Entries[Runtime.CurrentEntryIndex];

                if (entry.Type == NarrativeDialogueEntryType.Event)
                {
                    EventTriggered?.Invoke(entry.Event);
                    Runtime.CurrentEntryIndex++;
                    continue;
                }

                EntryChanged?.Invoke(entry);
                return true;
            }

            return false;
        }

        private void AdvanceTo(string nextNodeId, NarrativeNodeData fromNode)
        {
            NodeExited?.Invoke(fromNode);
            Runtime.MoveTo(nextNodeId);
        }

        private bool EvaluateCondition(NarrativeConditionData condition)
        {
            if (condition == null)
            {
                return false;
            }

            if (condition.Mode == NarrativeConditionMode.ClaimResolved)
            {
                if (!caseRuntime.TryGetClaim(condition.ClaimId, out ClaimRuntime claim))
                {
                    return false;
                }

                return condition.RequireSuccessful
                    ? claim.IsResolved && claim.WasSuccessful
                    : claim.IsResolved || claim.HasBeenAttempted;
            }

            int current = caseRuntime.CourtState.GetCourtStat(condition.Stat);

            return condition.Evaluate(current);
        }
    }
}
