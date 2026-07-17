using UnityEngine;
using Verdict.Editor.CaseFlow.Theme;
using Verdict.Editor.CaseFlow.Validation;
using Verdict.Systems.Validation;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseGraphBuilder
    {
        private readonly CaseGraphView graphView;

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

                float x = 100;
                float y = 100;

                foreach (FlowGraphNode node in session.FlowGraph.Nodes.Values)
                {
                    StatementContext context =
                        session.GetContext(node.Id);

                    StatementNodeView view =
                        graphView.CreateStatementNode(
                            context,
                            node,
                            new Vector2(x, y));

                    ApplyNodeTheme(
                        node,
                        view,
                        result);

                    x += 350;

                    if (x > 1800)
                    {
                        x = 100;
                        y += 250;
                    }
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
