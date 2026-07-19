using System;
using System.Collections.Generic;
using Verdict.Data.Narrative;

namespace Verdict.Runtime.Narrative
{
    /// <summary>
    /// Traversal state over a single NarrativeGraphData: which node we're
    /// on, and (for DialogueNodeData) which entry within that node.
    /// </summary>
    public sealed class NarrativeRuntime
    {
        private readonly Dictionary<string, NarrativeNodeData> nodeLookup;

        public NarrativeRuntime(NarrativeGraphData graph)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));

            nodeLookup = new Dictionary<string, NarrativeNodeData>(
                StringComparer.Ordinal);

            foreach (NarrativeNodeData node in graph.Nodes)
            {
                if (!string.IsNullOrWhiteSpace(node.NodeId))
                {
                    nodeLookup[node.NodeId] = node;
                }
            }

            CurrentNodeId = graph.StartNodeId;
        }

        public NarrativeGraphData Graph { get; }

        public string CurrentNodeId { get; internal set; }

        /// <summary>
        /// Index into the current DialogueNodeData's Entries. Meaningless
        /// for other node types.
        /// </summary>
        public int CurrentEntryIndex { get; internal set; }

        public NarrativeNodeData CurrentNode =>
            TryGetNode(CurrentNodeId, out NarrativeNodeData node)
                ? node
                : null;

        public bool IsFinished =>
            CurrentNode == null;

        public bool TryGetNode(
            string id,
            out NarrativeNodeData node)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                node = null;
                return false;
            }

            return nodeLookup.TryGetValue(id, out node);
        }

        internal bool MoveTo(string nextNodeId)
        {
            CurrentEntryIndex = 0;

            if (string.IsNullOrWhiteSpace(nextNodeId) ||
                !nodeLookup.ContainsKey(nextNodeId))
            {
                CurrentNodeId = null;
                return false;
            }

            CurrentNodeId = nextNodeId;
            return true;
        }
    }
}
