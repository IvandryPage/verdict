using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseGraphView : GraphView
    {
        private readonly Dictionary<string, StatementNodeView> nodeViews =
            new(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, StatementNodeView> NodeViews =>
            nodeViews;

        public event Action<StatementData> StatementSelected;

        public CaseGraphView()
        {
            style.flexGrow = 1;

            SetupZoom(
                ContentZoomer.DefaultMinScale,
                ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            Insert(0, grid);

            grid.StretchToParentSize();
        }

        public StatementNodeView CreateStatementNode(
            FlowGraphNode graphNode,
            Vector2 position)
        {
            StatementNodeView node =
                new StatementNodeView(graphNode.Statement);

            node.SetPosition(
                new Rect(
                    position,
                    new Vector2(300, 180)));

            node.Selected += HandleNodeSelected;

            AddElement(node);

            nodeViews.Add(
                graphNode.Id,
                node);

            return node;
        }

        public void CreateEdges(
            FlowGraph graph)
        {
            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                foreach (FlowGraphEdge edge in node.Outgoing)
                {
                    CreateEdgeView(edge);
                }
            }
        }

        private void CreateEdgeView(
            FlowGraphEdge edge)
        {
            if (!edge.IsResolved)
            {
                return;
            }

            if (!nodeViews.TryGetValue(edge.From.Id, out StatementNodeView from))
            {
                return;
            }

            if (!nodeViews.TryGetValue(edge.To.Id, out StatementNodeView to))
            {
                return;
            }

            Edge edgeView = new()
            {
                output = from.OutputPort,
                input = to.InputPort
            };

            edgeView.output.Connect(edgeView);
            edgeView.input.Connect(edgeView);

            AddElement(edgeView);
        }

        public void ClearGraph()
        {
            DeleteElements(graphElements.ToList());

            nodeViews.Clear();
        }

        private void HandleNodeSelected(
            StatementNodeView node)
        {
            StatementSelected?.Invoke(node.Statement);
        }
    }
}
