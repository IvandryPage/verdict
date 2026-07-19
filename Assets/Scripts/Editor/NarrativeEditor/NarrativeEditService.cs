using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Data.Narrative;
using Verdict.Editor.CaseEditor.Authoring;

namespace Verdict.Editor.NarrativeEditor
{
    /// <summary>
    /// Mutates a case's NarrativeGraphData with Undo/dirty-tracking,
    /// mirroring CaseEditService's pattern for the rest of the case.
    ///
    /// Structural mutations (add/remove node, add/remove choice, set
    /// start node) raise GraphModified so the window can rebuild the
    /// GraphView. Field-only edits (text, dropdowns, connections dragged
    /// directly in the GraphView) do not - the GraphView already reflects
    /// those itself, and rebuilding would steal keyboard focus mid-edit.
    /// </summary>
    public sealed class NarrativeEditService
    {
        private readonly CaseData caseData;

        public event Action GraphModified;

        public NarrativeEditService(CaseData caseData)
        {
            this.caseData = caseData
                ?? throw new ArgumentNullException(nameof(caseData));
        }

        public NarrativeGraphData Graph => caseData.Narrative;

        private void ExecuteStructural(string operation, Action action)
        {
            Undo.RecordObject(caseData, operation);
            action();
            EditorUtility.SetDirty(caseData);
            GraphModified?.Invoke();
        }

        private T ExecuteStructural<T>(string operation, Func<T> action)
        {
            Undo.RecordObject(caseData, operation);
            T result = action();
            EditorUtility.SetDirty(caseData);
            GraphModified?.Invoke();
            return result;
        }

        private void ExecuteField(string operation, Action action)
        {
            Undo.RecordObject(caseData, operation);
            action();
            EditorUtility.SetDirty(caseData);
        }

        public DialogueNodeData CreateDialogueNode(Vector2 position)
        {
            return ExecuteStructural("Create Dialogue Node", () =>
            {
                var node = new DialogueNodeData(AuthoringId.New(), position);
                Graph.AddNode(node);
                return node;
            });
        }

        public StatementNodeData CreateStatementNode(Vector2 position)
        {
            return ExecuteStructural("Create Statement Node", () =>
            {
                var node = new StatementNodeData(AuthoringId.New(), position);
                Graph.AddNode(node);
                return node;
            });
        }

        public ChoiceNodeData CreateChoiceNode(Vector2 position)
        {
            return ExecuteStructural("Create Choice Node", () =>
            {
                var node = new ChoiceNodeData(AuthoringId.New(), position);
                node.AddChoice("Choice 1");
                node.AddChoice("Choice 2");
                Graph.AddNode(node);
                return node;
            });
        }

        public ConditionNodeData CreateConditionNode(Vector2 position)
        {
            return ExecuteStructural("Create Condition Node", () =>
            {
                var node = new ConditionNodeData(AuthoringId.New(), position);
                Graph.AddNode(node);
                return node;
            });
        }

        public JumpNodeData CreateJumpNode(Vector2 position)
        {
            return ExecuteStructural("Create Jump Node", () =>
            {
                var node = new JumpNodeData(AuthoringId.New(), position);
                Graph.AddNode(node);
                return node;
            });
        }

        public EndNodeData CreateEndNode(Vector2 position)
        {
            return ExecuteStructural("Create End Node", () =>
            {
                var node = new EndNodeData(AuthoringId.New(), position);
                Graph.AddNode(node);
                return node;
            });
        }

        public GameplayNodeData CreateGameplayNode(Vector2 position)
        {
            return ExecuteStructural("Create Gameplay Node", () =>
            {
                var node = new GameplayNodeData(AuthoringId.New(), position);
                Graph.AddNode(node);
                return node;
            });
        }

        public void DeleteNode(NarrativeNodeData node)
        {
            if (node == null)
            {
                return;
            }

            ExecuteStructural("Delete Narrative Node", () => Graph.RemoveNode(node));
        }

        public void SetStartNode(NarrativeNodeData node)
        {
            ExecuteStructural("Set Start Node", () => Graph.SetStartNodeId(node?.NodeId));
        }

        public void MoveNode(NarrativeNodeData node, Vector2 position)
        {
            ExecuteField("Move Narrative Node", () => node.SetPosition(position));
        }

        public void SetOutgoing(NarrativeNodeData node, string outputKey, string targetNodeId)
        {
            ExecuteField("Connect Narrative Node", () => ApplyOutgoing(node, outputKey, targetNodeId));
        }

        public void ClearOutgoing(NarrativeNodeData node, string outputKey)
        {
            ExecuteField("Disconnect Narrative Node", () => ApplyOutgoing(node, outputKey, null));
        }

        private static void ApplyOutgoing(NarrativeNodeData node, string outputKey, string targetNodeId)
        {
            switch (node)
            {
                case DialogueNodeData dialogueNode:
                    dialogueNode.SetNextNodeId(targetNodeId);
                    break;

                case StatementNodeData statementNode:
                    statementNode.SetNextNodeId(targetNodeId);
                    break;

                case JumpNodeData jumpNode:
                    jumpNode.SetTargetNodeId(targetNodeId);
                    break;

                case GameplayNodeData gameplayNode:
                    gameplayNode.SetNextNodeId(targetNodeId);
                    break;

                case ConditionNodeData conditionNode:

                    if (outputKey == NarrativeOutputKeys.ConditionTrue)
                    {
                        conditionNode.SetTrueNodeId(targetNodeId);
                    }
                    else if (outputKey == NarrativeOutputKeys.ConditionFalse)
                    {
                        conditionNode.SetFalseNodeId(targetNodeId);
                    }

                    break;

                case ChoiceNodeData choiceNode:

                    NarrativeChoiceOptionData choice = choiceNode.Choices
                        .FirstOrDefault(c => c.ChoiceId == outputKey);

                    choice?.SetNextNodeId(targetNodeId);

                    break;
            }
        }

        public void AddChoice(ChoiceNodeData node)
        {
            ExecuteStructural("Add Choice", () => node.AddChoice($"Choice {node.Choices.Count + 1}"));
        }

        public void RemoveChoiceAt(ChoiceNodeData node, int index)
        {
            ExecuteStructural("Remove Choice", () => node.RemoveChoiceAt(index));
        }

        public void RemoveChoice(ChoiceNodeData node, NarrativeChoiceOptionData choice)
        {
            ExecuteStructural("Remove Choice", () => node.RemoveChoice(choice));
        }

        public void SetChoiceText(NarrativeChoiceOptionData choice, string text)
        {
            ExecuteField("Edit Choice Text", () => choice.SetText(text));
        }

        public void SetStatementId(StatementNodeData node, string statementId)
        {
            ExecuteField("Set Statement Reference", () => node.SetStatementId(statementId));
        }

        public void SetEndingId(EndNodeData node, string endingId)
        {
            ExecuteField("Set Ending Reference", () => node.SetEndingId(endingId));
        }

        public void SetGameplayEventId(GameplayNodeData node, string eventId)
        {
            ExecuteField("Set Gameplay Event Id", () => node.SetGameplayEventId(eventId));
        }

        public void SetCourtStatCondition(ConditionNodeData node, CourtStat stat, NarrativeComparisonOperator op, int value)
        {
            ExecuteField("Edit Condition", () =>
            {
                node.Condition.SetMode(NarrativeConditionMode.CourtStat);
                node.Condition.SetStat(stat);
                node.Condition.SetOperator(op);
                node.Condition.SetValue(value);
            });
        }

        public void SetClaimResolvedCondition(ConditionNodeData node, string claimId, bool requireSuccessful)
        {
            ExecuteField("Edit Condition", () =>
            {
                node.Condition.SetMode(NarrativeConditionMode.ClaimResolved);
                node.Condition.SetClaimId(claimId);
                node.Condition.SetRequireSuccessful(requireSuccessful);
            });
        }

        public void AddDialogueEntry(DialogueNodeData node, NarrativeDialogueEntryData entry)
        {
            ExecuteField("Add Dialogue Entry", () => node.AddEntry(entry));
        }

        public void RemoveDialogueEntryAt(DialogueNodeData node, int index)
        {
            ExecuteField("Remove Dialogue Entry", () => node.RemoveEntryAt(index));
        }

        public void MoveDialogueEntry(DialogueNodeData node, int from, int to)
        {
            ExecuteField("Reorder Dialogue Entry", () => node.MoveEntry(from, to));
        }

        public void RecordDialogueEntryEdit()
        {
            ExecuteField("Edit Dialogue Entry", () => { });
        }
    }
}
