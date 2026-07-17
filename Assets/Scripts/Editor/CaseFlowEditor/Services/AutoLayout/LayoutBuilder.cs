using System.Collections.Generic;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow.Layout
{
    public sealed class LayerBuilder
    {
        public Dictionary<string, int> Build(
            FlowGraph graph)
        {
            Dictionary<string, int> layers =
                new();

            Queue<FlowGraphNode> queue =
                new();

            foreach (FlowGraphNode node in graph.Nodes.Values)
            {
                if (!node.IsEntry)
                    continue;

                layers[node.Id] = 0;

                queue.Enqueue(node);
            }

            while (queue.Count > 0)
            {
                FlowGraphNode current =
                    queue.Dequeue();

                int currentLayer =
                    layers[current.Id];

                foreach (FlowGraphEdge edge in current.Outgoing)
                {
                    if (!edge.IsResolved)
                        continue;

                    FlowGraphNode next =
                        edge.To;

                    int nextLayer =
                        currentLayer + 1;

                    if (!layers.TryGetValue(
                            next.Id,
                            out int existing)
                        || nextLayer > existing)
                    {
                        layers[next.Id] = nextLayer;

                        queue.Enqueue(next);
                    }
                }
            }

            return layers;
        }
    }
}
