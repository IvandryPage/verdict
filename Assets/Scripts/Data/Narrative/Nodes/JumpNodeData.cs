using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Unconditional redirect to another node. Useful for merging
    /// branches back together or looping without duplicating nodes.
    /// </summary>
    [Serializable]
    public sealed class JumpNodeData : NarrativeNodeData
    {
        [SerializeField]
        private string targetNodeId;

        public JumpNodeData()
        {
        }

        public JumpNodeData(string nodeId, Vector2 position)
            : base(nodeId, position)
        {
        }

        public string TargetNodeId => targetNodeId;

        public void SetTargetNodeId(string id)
        {
            targetNodeId = id;
        }

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            if (!string.IsNullOrWhiteSpace(targetNodeId))
            {
                yield return targetNodeId;
            }
        }
    }
}
