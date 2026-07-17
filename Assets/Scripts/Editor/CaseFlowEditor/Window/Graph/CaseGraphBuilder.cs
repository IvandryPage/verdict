using System.Collections.Generic;
using UnityEngine;
using Verdict.Editor.CaseFlow.Service;
using Verdict.Editor.CaseFlow.Theme;
using Verdict.Editor.CaseFlow.Validation;
using Verdict.Systems.Validation;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseGraphBuilder
    {
        private readonly CaseGraphView graphView;

        private readonly AutoLayoutService autoLayout =
            new();

        public CaseGraphBuilder(
            CaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void Build(
            EditorSession session,
            ValidationResult result)
        {
            graphView.BeginRebuild();

            try
            {
                graphView.ClearGraph();

                Dictionary<string, Vector2> positions =
                    autoLayout.Calculate(
                        session.FlowGraph);

                foreach (FlowGraphNode node in session.FlowGraph.Nodes.Values)
                {
                    StatementContext context =
                        session.GetContext(node.Id);

                    StatementNodeView view = graphView.CreateStatementNode(
                        context,
                        node,
                        positions[node.Id]);

                    ApplyNodeTheme(
                        node,
                        view,
                        result);
                }

                graphView.CreateEdges(session.FlowGraph);
            }
            finally
            {
                graphView.EndRebuild();
            }
        }

        private static void ApplyNodeTheme(
            FlowGraphNode node,
            StatementNodeView view,
            ValidationResult result)
        {
            NodeColor color =
                node.IsEntry
                    ? NodeColor.Entry
                    : NodeColor.Hidden;

            NodeColor validation =
                ValidationOverlayBuilder.GetNodeColor(
                    result,
                    node.Id);

            if (validation != NodeColor.Default)
            {
                color = validation;
            }

            view.ApplyStyle(
                CaseEditorTheme.GetNodeStyle(color));
        }
    }
}
