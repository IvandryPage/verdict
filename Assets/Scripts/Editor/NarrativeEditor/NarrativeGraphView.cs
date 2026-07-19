using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Narrative;

namespace Verdict.Editor.NarrativeEditor
{
    public sealed class NarrativeGraphView : GraphView
    {
        private readonly NarrativeEditService editService;

        private bool suppressGraphEvents;

        private readonly Dictionary<string, NarrativeNodeView> nodeViews =
            new(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, NarrativeNodeView> NodeViews => nodeViews;

        public event Action<NarrativeNodeView> NodeSelected;

        public NarrativeGraphView(NarrativeEditService editService)
        {
            this.editService = editService;

            style.flexGrow = 1;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphViewChanged = HandleGraphChanged;

            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports
                .Where(port =>
                    port.direction != startPort.direction &&
                    port.node != startPort.node)
                .ToList();
        }

        public void ClearGraph()
        {
            List<GraphElement> elements = graphElements.ToList();
            nodeViews.Clear();
            DeleteElements(elements);
        }

        public NarrativeNodeView AddNodeView(NarrativeNodeData data, NarrativeGraphContext context)
        {
            NarrativeNodeView view = new(data, editService, context);

            view.SetPosition(new Rect(data.Position, new Vector2(240, 160)));
            view.Selected += HandleNodeSelected;

            AddElement(view);
            nodeViews[data.NodeId] = view;

            return view;
        }

        public void ConnectPorts(Port output, Port input)
        {
            if (output == null || input == null)
            {
                return;
            }

            bool alreadyConnected = edges.ToList()
                .Any(e => e.output == output && e.input == input);

            if (alreadyConnected)
            {
                return;
            }

            Edge edge = new() { output = output, input = input };
            edge.output.Connect(edge);
            edge.input.Connect(edge);

            AddElement(edge);
        }

        /// <summary>
        /// Removes a specific output port from a node, disconnecting and
        /// removing any edge attached to it. Used when Choice options are
        /// removed - the node's other ports are left untouched.
        /// </summary>
        public void DisconnectAndRemovePort(NarrativeNodeView nodeView, Port port)
        {
            List<Edge> connected = edges.ToList()
                .Where(e => e.output == port)
                .ToList();

            foreach (Edge edge in connected)
            {
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
                RemoveElement(edge);
            }

            nodeView.outputContainer.Remove(port);
        }

        /// <summary>
        /// Re-creates edges for a node's current output slots from data,
        /// after its ports were rebuilt (e.g. Choice add/remove). Only
        /// touches this node's outgoing edges.
        /// </summary>
        public void ReconnectNode(NarrativeNodeView nodeView)
        {
            NarrativeGraphBuilder.ConnectNodeOutputs(this, nodeView, nodeViews);
        }

        public void SetStartNode(NarrativeNodeView nodeView)
        {
            editService.SetStartNode(nodeView.Data);
        }

        private void HandleNodeSelected(NarrativeNodeView node)
        {
            NodeSelected?.Invoke(node);
        }

        private GraphViewChange HandleGraphChanged(GraphViewChange change)
        {
            if (suppressGraphEvents)
            {
                return change;
            }

            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate.ToArray())
                {
                    HandleEdgeCreated(edge);
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (GraphElement element in change.elementsToRemove.ToArray())
                {
                    switch (element)
                    {
                        case Edge edge:
                            HandleEdgeRemoved(edge);
                            break;

                        case NarrativeNodeView nodeView:
                            editService.DeleteNode(nodeView.Data);
                            nodeViews.Remove(nodeView.Data.NodeId);
                            break;
                    }
                }
            }

            if (change.movedElements != null)
            {
                foreach (GraphElement element in change.movedElements)
                {
                    if (element is NarrativeNodeView nodeView)
                    {
                        editService.MoveNode(nodeView.Data, nodeView.GetPosition().position);
                    }
                }
            }

            return change;
        }

        private void HandleEdgeCreated(Edge edge)
        {
            if (edge.output?.node is not NarrativeNodeView from ||
                edge.input?.node is not NarrativeNodeView to)
            {
                return;
            }

            string outputKey = from.OutputSlots
                .FirstOrDefault(slot => slot.Port == edge.output)
                .Key;

            if (string.IsNullOrEmpty(outputKey))
            {
                return;
            }

            editService.SetOutgoing(from.Data, outputKey, to.Data.NodeId);
        }

        private void HandleEdgeRemoved(Edge edge)
        {
            if (edge.output?.node is not NarrativeNodeView from)
            {
                return;
            }

            string outputKey = from.OutputSlots
                .FirstOrDefault(slot => slot.Port == edge.output)
                .Key;

            if (string.IsNullOrEmpty(outputKey))
            {
                return;
            }

            editService.ClearOutgoing(from.Data, outputKey);
        }

        public void BeginRebuild()
        {
            suppressGraphEvents = true;
        }

        public void EndRebuild()
        {
            schedule.Execute(() => suppressGraphEvents = false);
        }
    }
}
