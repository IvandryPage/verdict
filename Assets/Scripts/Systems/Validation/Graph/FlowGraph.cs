using System;
using System.Collections.Generic;
using System.Linq;

namespace Verdict.Systems.Validation.Graph
{
    public sealed class FlowGraph
    {
        private readonly Dictionary<string, FlowGraphNode> _nodes =
            new(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, FlowGraphNode> Nodes => _nodes;

        public void AddNode(FlowGraphNode node)
        {
            if (!_nodes.TryAdd(node.Id, node))
            {
                throw new InvalidOperationException(
                    $"Duplicate graph node '{node.Id}'.");
            }
        }

        public bool TryGetNode(
            string id,
            out FlowGraphNode node)
        {
            return _nodes.TryGetValue(id, out node);
        }
    }

}
