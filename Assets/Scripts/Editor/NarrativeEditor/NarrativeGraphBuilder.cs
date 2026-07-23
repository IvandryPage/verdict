using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Data.Narrative;

namespace Verdict.Editor.NarrativeEditor
{
    public static class NarrativeGraphBuilder
    {
        public static void Build(
            NarrativeGraphView graphView,
            CaseData caseData,
            NarrativeGraphContext context)
        {
            graphView.BeginRebuild();

            try
            {
                graphView.ClearGraph();

                NarrativeGraphData graph = caseData.Narrative;

                foreach (NarrativeNodeData node in graph.Nodes)
                {
                    NarrativeNodeView view = graphView.AddNodeView(node, context);
                    view.SetIsStartNode(node.NodeId == graph.StartNodeId);
                }

                foreach (NarrativeNodeView view in graphView.NodeViews.Values)
                {
                    ConnectNodeOutputs(graphView, view, graphView.NodeViews);
                }
            }
            finally
            {
                graphView.EndRebuild();
            }
        }

        /// <summary>
        /// Creates edges for a single node's current output slots based on
        /// the underlying data. Safe to call repeatedly - ConnectPorts
        /// skips edges that already exist.
        /// </summary>
        public static void ConnectNodeOutputs(
            NarrativeGraphView graphView,
            NarrativeNodeView nodeView,
            IReadOnlyDictionary<string, NarrativeNodeView> nodeViewsById)
        {
            foreach ((string key, var port) in nodeView.OutputSlots)
            {
                string targetId = GetOutgoingTarget(nodeView.Data, key);

                if (string.IsNullOrWhiteSpace(targetId))
                {
                    continue;
                }

                if (!nodeViewsById.TryGetValue(targetId, out NarrativeNodeView targetView))
                {
                    continue;
                }

                graphView.ConnectPorts(port, targetView.InputPort);
            }
        }

        private static string GetOutgoingTarget(NarrativeNodeData node, string outputKey)
        {
            return node switch
            {
                DialogueNodeData dialogueNode => dialogueNode.NextNodeId,
                StatementNodeData statementNode => statementNode.NextNodeId,
                JumpNodeData jumpNode => jumpNode.TargetNodeId,
                GameplayNodeData gameplayNode => gameplayNode.NextNodeId,

                ConditionNodeData conditionNode =>
                    outputKey == NarrativeOutputKeys.ConditionTrue
                        ? conditionNode.TrueNodeId
                        : conditionNode.FalseNodeId,

                ChoiceNodeData choiceNode =>
                    choiceNode.Choices.FirstOrDefault(c => c.ChoiceId == outputKey)?.NextNodeId,

                _ => null
            };
        }
    }
}
