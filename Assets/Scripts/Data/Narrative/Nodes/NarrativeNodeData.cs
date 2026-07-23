using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Base type for every node in a NarrativeGraphData.
    /// Knows nothing about gameplay - a node only knows its own id,
    /// its editor position (for the future graph editor), and which
    /// node id(s) it can lead to.
    /// </summary>
    [Serializable]
    public abstract class NarrativeNodeData
    {
        [SerializeField] private string nodeId;

        [SerializeField] private Vector2 position;

        protected NarrativeNodeData()
        {
        }

        protected NarrativeNodeData(string nodeId, Vector2 position)
        {
            this.nodeId = nodeId;
            this.position = position;
        }

        public string NodeId => nodeId;

        public Vector2 Position => position;

        /// <summary>
        /// Used by the graph editor when a node is dragged. Not meant to
        /// be called at runtime.
        /// </summary>
        public void SetPosition(Vector2 newPosition)
        {
            position = newPosition;
        }

        /// <summary>
        /// All node ids this node can advance to. Empty/null entries mean
        /// "no outgoing connection here" and are filtered out by callers.
        /// </summary>
        public abstract IEnumerable<string> GetOutgoingNodeIds();
    }
}
