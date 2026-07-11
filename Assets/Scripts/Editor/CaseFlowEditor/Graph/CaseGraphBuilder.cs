using UnityEngine;
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
            FlowGraph graph)
        {
            graphView.ClearGraph();

            float x = 100;
            float y = 100;

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                graphView.CreateStatementNode(
                    node,
                    new Vector2(x, y));

                x += 350;

                if (x > 1800)
                {
                    x = 100;
                    y += 250;
                }
            }

            graphView.CreateEdges(graph);
        }
    }
}
