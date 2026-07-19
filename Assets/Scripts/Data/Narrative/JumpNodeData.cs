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

        public string TargetNodeId => targetNodeId;

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            if (!string.IsNullOrWhiteSpace(targetNodeId))
            {
                yield return targetNodeId;
            }
        }
    }
}
