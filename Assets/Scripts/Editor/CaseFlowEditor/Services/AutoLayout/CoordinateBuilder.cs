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

            IEnumerable<IGrouping<int, KeyValuePair<string, int>>> grouped =
                layers
                .GroupBy(x => x.Value)
                .OrderBy(x => x.Key);

            foreach (IGrouping<int, KeyValuePair<string, int>> layer in grouped)
            {
                List<KeyValuePair<string, int>> nodes =
                    layer.ToList();

                nodes.Sort((a, b) =>
                {
                    float ay =
                        AverageParentY(
                            a.Key,
                            incoming,
                            positions);

                    float by =
                        AverageParentY(
                            b.Key,
                            incoming,
                            positions);

                    return ay.CompareTo(by);
                });

                int count =
                    nodes.Count;

                float offset =
                    (count - 1) *
                    settings.RowSpacing *
                    0.5f;

                for (int row = 0;
                     row < nodes.Count;
                     row++)
                {
                    KeyValuePair<string, int> node =
                        nodes[row];

                    float x =
                        settings.StartPosition.x +
                        layer.Key *
                        settings.LayerSpacing;

                    float y =
                        settings.StartPosition.y +
                        row *
                        settings.RowSpacing -
                        offset;

                    positions[node.Key] =
                        new Vector2(x, y);
                }
            }

            RelaxPositions(
                graph,
                incoming,
                positions);


            ResolveCollisions(
                graph,
                positions);

            CenterParents(
                graph,
                positions);

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
            FlowGraph graph,
            Dictionary<string, Vector2> positions)
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
        }

        private static void CenterParents(
            FlowGraph graph,
            Dictionary<string, Vector2> positions)
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
