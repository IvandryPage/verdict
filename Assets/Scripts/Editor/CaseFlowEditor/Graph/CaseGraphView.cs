using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseFlow.Theme;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseGraphView : GraphView
    {
        private readonly Dictionary<string, StatementNodeView> nodeViews =
            new(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, StatementNodeView> NodeViews =>
            nodeViews;

        public event Action<StatementContext> StatementSelected;

        public event Action<Edge> EdgeCreated;
        public event Action<Edge> EdgeRemoved;

        public CaseGraphView()
        {
            style.flexGrow = 1;

            SetupZoom(
                ContentZoomer.DefaultMinScale,
                ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(
                new SelectionDragger());

            this.graphViewChanged =
                HandleGraphChanged;

            GridBackground grid = new();
            Insert(0, grid);

            grid.StretchToParentSize();
        }

        public StatementNodeView CreateStatementNode(
            StatementContext context,
            FlowGraphNode graphNode,
            Vector2 position)
        {
            StatementNodeView node =
                new StatementNodeView(context);

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

            EdgeStyle style = GetStyle(edge.Type);

            ApplyStyle(edgeView, style);

            edgeView.UpdateEdgeControl();
            edgeView.edgeControl.MarkDirtyRepaint();
        }

        // FIXME: There's a bug where styles doesn't applied to the graph edge.
        // Report says it is because the properties got ignored
        private static void ApplyStyle(
            Edge edge,
            EdgeStyle style)
        {
            edge.edgeControl.inputColor =
                style.Color;

            edge.edgeControl.outputColor =
                style.Color;

            edge.edgeControl.edgeWidth =
                (int)style.Width;
        }

        private static EdgeStyle GetStyle(
        FlowEdgeType type)
        {
            EdgeColor color =
                type switch
                {
                    FlowEdgeType.NextStatement =>
                        EdgeColor.Default,

                    FlowEdgeType.RevealStatement =>
                        EdgeColor.Reveal,

                    FlowEdgeType.RevealWitness =>
                        EdgeColor.Reveal,

                    FlowEdgeType.RevealTestimony =>
                        EdgeColor.Reveal,

                    FlowEdgeType.JumpStatement =>
                        EdgeColor.Jump,

                    FlowEdgeType.JumpWitness =>
                        EdgeColor.Jump,

                    FlowEdgeType.JumpTestimony =>
                        EdgeColor.Jump,

                    _ =>
                        EdgeColor.Default
                };

            return CaseEditorTheme.GetEdgeStyle(
                color);
        }

        public void Frame(
            string id)
        {
            if (!nodeViews.TryGetValue(
                id,
                out StatementNodeView node))
            {
                return;
            }

            ClearSelection();

            AddToSelection(node);

            FrameSelection();
        }

        public void ClearGraph()
        {
            DeleteElements(graphElements.ToList());

            nodeViews.Clear();
        }

        private void HandleNodeSelected(
            StatementNodeView node)
        {
            StatementSelected?.Invoke(node.Context);
        }

        private GraphViewChange HandleGraphChanged(
            GraphViewChange change)
        {
            Debug.Log("Graph Changed");
            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate)
                {
                    EdgeCreated?.Invoke(edge);
                }
            }


            if (change.elementsToRemove != null)
            {
                foreach (GraphElement element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        EdgeRemoved?.Invoke(edge);
                    }
                }
            }


            return change;
        }

        public override List<Port> GetCompatiblePorts(
            Port startPort,
            NodeAdapter nodeAdapter)
        {
            return ports
                .Where(port =>
                    port.direction != startPort.direction &&
                    port.node != startPort.node)
                .ToList();
        }
    }
}
