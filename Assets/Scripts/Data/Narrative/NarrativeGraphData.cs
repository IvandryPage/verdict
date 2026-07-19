using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// A case's whole narrative: dialogue, statement pause points, choices,
    /// conditions, jumps and endings, wired together as a graph.
    ///
    /// This replaces the old per-statement dialogue binding. Gameplay
    /// (Case/Witness/Testimony/Statement) does not know this exists -
    /// only StatementNodeData reaches into gameplay, by StatementId.
    /// </summary>
    [Serializable]
    public sealed class NarrativeGraphData
    {
        [SerializeField]
        private string startNodeId;

        [SerializeField, SerializeReference]
        private List<NarrativeNodeData> nodes = new();

        public string StartNodeId => startNodeId;

        public IReadOnlyList<NarrativeNodeData> Nodes => nodes;

        public void SetStartNodeId(string nodeId)
        {
            startNodeId = nodeId;
        }

        public void AddNode(NarrativeNodeData node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            nodes.Add(node);
        }

        public bool RemoveNode(NarrativeNodeData node)
        {
            if (node == null)
            {
                return false;
            }

            bool removed = nodes.Remove(node);

            if (removed && startNodeId == node.NodeId)
            {
                startNodeId = null;
            }

            return removed;
        }

        public NarrativeNodeData FindNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            foreach (NarrativeNodeData node in nodes)
            {
                if (node != null && node.NodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        public void Clear()
        {
            nodes.Clear();
            startNodeId = null;
        }
    }
}
