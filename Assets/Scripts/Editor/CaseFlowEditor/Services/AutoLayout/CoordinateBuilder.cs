using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow.Layout
{
    public sealed class CoordinateBuilder
    {
        private const int RelaxationPasses = 3;

        public Dictionary<string, Vector2> Build(
            FlowGraph graph,
            Dictionary<string, int> layers,
            LayoutSettings settings)
        {
            Dictionary<string, Vector2> positions =
                new();

            Dictionary<string, List<FlowGraphNode>> incoming =
                BuildIncomingMap(graph);

            Dictionary<string, List<FlowGraphNode>> outgoing =
                BuildOutgoingMap(graph);

            Dictionary<int, List<NodeLayout>> layerNodes =
                BuildLayerNodes(graph, layers);

            InitializeLayerOrder(
                layerNodes,
                incoming);

            MinimizeCrossings(
                layerNodes,
                incoming,
                outgoing);

            AssignCoordinates(
                layerNodes,
                settings,
                positions);

            ResolveCollisions(
                positions,
                settings.RowSpacing);

            CenterParents(
                graph,
                positions,
                settings);

            return positions;
        }

        private static Dictionary<string, List<FlowGraphNode>> BuildIncomingMap(
            FlowGraph graph)
        {
            Dictionary<string, List<FlowGraphNode>> map =
                new();

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                map[node.Id] =
                    new List<FlowGraphNode>();
            }

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                foreach (FlowGraphEdge edge in node.Outgoing)
                {
                    if (!edge.IsResolved)
                        continue;

                    map[edge.To.Id].Add(node);
                }
            }

            return map;
        }

        private static Dictionary<string, List<FlowGraphNode>> BuildOutgoingMap(
            FlowGraph graph)
        {
            Dictionary<string, List<FlowGraphNode>> map =
                new();

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                map[node.Id] =
                    new List<FlowGraphNode>();
            }

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                foreach (FlowGraphEdge edge in node.Outgoing)
                {
                    if (!edge.IsResolved)
                        continue;

                    map[node.Id].Add(edge.To);
                }
            }

            return map;
        }

        private static Dictionary<int, List<NodeLayout>> BuildLayerNodes(
            FlowGraph graph,
            Dictionary<string, int> layers)
        {
            Dictionary<int, List<NodeLayout>> layerNodes =
                new();

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                int layer =
                    layers.TryGetValue(node.Id, out int value)
                        ? value
                        : 0;

                if (!layerNodes.TryGetValue(
                        layer,
                        out List<NodeLayout> list))
                {
                    list = new List<NodeLayout>();
                    layerNodes.Add(layer, list);
                }

                list.Add(new NodeLayout
                {
                    Id = node.Id,
                    Layer = layer
                });
            }

            foreach (List<NodeLayout> list in layerNodes.Values)
            {
                list.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            }

            return layerNodes;
        }

        private static void InitializeLayerOrder(
            Dictionary<int, List<NodeLayout>> layerNodes,
            Dictionary<string, List<FlowGraphNode>> incoming)
        {
            Dictionary<string, NodeLayout> layoutById =
                layerNodes.Values
                    .SelectMany(list => list)
                    .ToDictionary(node => node.Id);

            foreach (KeyValuePair<int, List<NodeLayout>> layer in layerNodes)
            {
                layer.Value.Sort((a, b) =>
                {
                    float ay =
                        AverageParentOrder(a.Id, incoming, layoutById);
                    float by =
                        AverageParentOrder(b.Id, incoming, layoutById);
                    return ay.CompareTo(by);
                });

                for (int i = 0; i < layer.Value.Count; i++)
                {
                    layer.Value[i].Order = i;
                }
            }
        }

        private static float AverageParentOrder(
            string nodeId,
            Dictionary<string, List<FlowGraphNode>> incoming,
            Dictionary<string, NodeLayout> layoutById)
        {
            if (!incoming.TryGetValue(nodeId, out List<FlowGraphNode> parents) ||
                parents.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            int count = 0;
            foreach (FlowGraphNode parent in parents)
            {
                if (layoutById.TryGetValue(parent.Id, out NodeLayout parentLayout))
                {
                    total += parentLayout.Order;
                    count++;
                }
            }

            return count == 0 ? 0f : total / count;
        }

        private static void MinimizeCrossings(
            Dictionary<int, List<NodeLayout>> layerNodes,
            Dictionary<string, List<FlowGraphNode>> incoming,
            Dictionary<string, List<FlowGraphNode>> outgoing)
        {
            int minLayer = layerNodes.Keys.Min();
            int maxLayer = layerNodes.Keys.Max();

            for (int pass = 0; pass < 4; pass++)
            {
                bool topDown = pass % 2 == 0;

                if (topDown)
                {
                    for (int layer = minLayer + 1; layer <= maxLayer; layer++)
                    {
                        if (!layerNodes.TryGetValue(layer, out List<NodeLayout> list))
                            continue;

                        OrderLayerByAdjacent(
                            list,
                            layerNodes[layer - 1],
                            incoming);
                    }
                }
                else
                {
                    for (int layer = maxLayer - 1; layer >= minLayer; layer--)
                    {
                        if (!layerNodes.TryGetValue(layer, out List<NodeLayout> list))
                            continue;

                        OrderLayerByAdjacent(
                            list,
                            layerNodes[layer + 1],
                            outgoing);
                    }
                }
            }
        }

        private static void OrderLayerByAdjacent(
            List<NodeLayout> layer,
            List<NodeLayout> adjacentLayer,
            Dictionary<string, List<FlowGraphNode>> adjacency)
        {
            if (adjacentLayer == null || adjacentLayer.Count == 0)
                return;

            Dictionary<string, int> adjacentOrder =
                adjacentLayer
                    .Select((layout, index) => new { layout.Id, index })
                    .ToDictionary(x => x.Id, x => x.index);

            List<(NodeLayout node, float median)> scoring =
                layer.Select(node =>
                {
                    List<int> neighborOrders =
                        adjacency.TryGetValue(node.Id, out List<FlowGraphNode> neighbors)
                            ? neighbors
                                .Where(n => adjacentOrder.ContainsKey(n.Id))
                                .Select(n => adjacentOrder[n.Id])
                                .OrderBy(x => x)
                                .ToList()
                            : new List<int>();

                    float median = neighborOrders.Count switch
                    {
                        0 => node.Order,
                        1 => neighborOrders[0],
                        _ => (neighborOrders[(neighborOrders.Count - 1) / 2] + neighborOrders[neighborOrders.Count / 2]) * 0.5f,
                    };

                    return (node, median);
                })
                .ToList();

            scoring.Sort((a, b) =>
            {
                int compare = a.median.CompareTo(b.median);
                return compare != 0 ? compare : string.CompareOrdinal(a.node.Id, b.node.Id);
            });

            for (int i = 0; i < scoring.Count; i++)
            {
                scoring[i].node.Order = i;
                layer[i] = scoring[i].node;
            }
        }

        private static void AssignCoordinates(
            Dictionary<int, List<NodeLayout>> layerNodes,
            LayoutSettings settings,
            Dictionary<string, Vector2> positions)
        {
            foreach (KeyValuePair<int, List<NodeLayout>> layer in layerNodes)
            {
                List<NodeLayout> nodes = layer.Value;
                int count = nodes.Count;
                float offset = (count - 1) * settings.RowSpacing * 0.5f;

                for (int i = 0; i < count; i++)
                {
                    NodeLayout node = nodes[i];
                    float x = settings.StartPosition.x + layer.Key * settings.LayerSpacing;
                    float y = settings.StartPosition.y + node.Order * settings.RowSpacing - offset;
                    node.Position = new Vector2(x, y);
                    positions[node.Id] = node.Position;
                }
            }
        }

        private static float AverageParentY(
            string nodeId,
            Dictionary<string, List<FlowGraphNode>> incoming,
            Dictionary<string, Vector2> positions)
        {
            List<FlowGraphNode> parents =
                incoming[nodeId];

            if (parents.Count == 0)
                return 0f;

            float total = 0f;
            int count = 0;

            foreach (FlowGraphNode parent in parents)
            {
                if (!positions.TryGetValue(
                        parent.Id,
                        out Vector2 position))
                {
                    continue;
                }

                total +=
                    position.y;

                count++;
            }

            if (count == 0)
                return 0f;

            return total / count;
        }

        private void RelaxPositions(
            FlowGraph graph,
            Dictionary<string, List<FlowGraphNode>> incoming,
            Dictionary<string, Vector2> positions)
        {
            for (int pass = 0;
                pass < RelaxationPasses;
                pass++)
            {
                bool reverse =
                    pass % 2 == 1;

                IEnumerable<FlowGraphNode> nodes =
                    reverse
                    ? graph.Nodes.Values.Reverse()
                    : graph.Nodes.Values;

                foreach (FlowGraphNode node in nodes)
                {
                    RelaxNode(
                        node,
                        incoming,
                        positions);
                }
            }
        }

        private static void RelaxNode(
            FlowGraphNode node,
            Dictionary<string, List<FlowGraphNode>> incoming,
            Dictionary<string, Vector2> positions)
        {
            if (!positions.TryGetValue(
                node.Id,
                out Vector2 current))
            {
                return;
            }

            float target =
                AverageParentY(
                    node.Id,
                    incoming,
                    positions);

            current.y =
                Mathf.Lerp(
                    current.y,
                    target,
                    0.5f);

            positions[node.Id] =
                current;
        }

        private static void ResolveCollisions(
            Dictionary<string, Vector2> positions,
            float rowSpacing)
        {
            Dictionary<float, List<KeyValuePair<string, Vector2>>> layers =
                new();

            foreach (KeyValuePair<string, Vector2> pair in positions)
            {
                float x = pair.Value.x;

                if (!layers.TryGetValue(x, out var list))
                {
                    list = new();
                    layers.Add(x, list);
                }

                list.Add(pair);
            }

            foreach (List<KeyValuePair<string, Vector2>> layer in layers.Values)
            {
                layer.Sort((a, b) =>
                    a.Value.y.CompareTo(b.Value.y));

                for (int i = 1; i < layer.Count; i++)
                {
                    Vector2 previous =
                        positions[layer[i - 1].Key];

                    Vector2 current =
                        positions[layer[i].Key];

                    float minY =
                        previous.y + 180f;

                    if (current.y < minY)
                    {
                        current.y = minY;

                        positions[layer[i].Key] =
                            current;
                    }
                }
            }
            foreach (List<KeyValuePair<string, Vector2>> layer in layers.Values)
            {
                layer.Sort((a, b) =>
                    a.Value.y.CompareTo(b.Value.y));

                for (int i = 1; i < layer.Count; i++)
                {
                    Vector2 previous =
                        positions[layer[i - 1].Key];

                    Vector2 current =
                        positions[layer[i].Key];

                    float minY =
                        previous.y + rowSpacing;

                    if (current.y < minY)
                    {
                        current.y = minY;
                        positions[layer[i].Key] = current;
                    }
                }
            }
        }

        private static void CenterParents(
            FlowGraph graph,
            Dictionary<string, Vector2> positions,
            LayoutSettings settings)
        {
            foreach (FlowGraphNode node in graph.Nodes.Values.Reverse())
            {
                CenterParent(
                    node,
                    positions);
            }
        }

        private static void CenterParent(
            FlowGraphNode node,
            Dictionary<string, Vector2> positions)
        {
            List<FlowGraphEdge> edges =
                node.Outgoing
                    .Where(x => x.IsResolved)
                    .ToList();

            if (edges.Count == 0)
                return;

            float total = 0f;
            int count = 0;

            foreach (FlowGraphEdge edge in edges)
            {
                if (!positions.TryGetValue(
                        edge.To.Id,
                        out Vector2 child))
                {
                    continue;
                }

                total += child.y;
                count++;
            }

            if (count == 0)
                return;

            Vector2 current =
                positions[node.Id];

            float target =
                total / count;

            current.y =
                Mathf.Lerp(
                    current.y,
                    target,
                    0.5f);

            positions[node.Id] =
                current;
        }
    }
}
