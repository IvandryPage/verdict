using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Evaluates a condition against CourtState and advances to
    /// TrueNodeId or FalseNodeId. Resolved automatically - never pauses
    /// for the player.
    /// </summary>
    [Serializable]
    public sealed class ConditionNodeData : NarrativeNodeData
    {
        [SerializeField]
        private NarrativeConditionData condition = new();

        [SerializeField]
        private string trueNodeId;

        [SerializeField]
        private string falseNodeId;

        public NarrativeConditionData Condition => condition;

        public string TrueNodeId => trueNodeId;

        public string FalseNodeId => falseNodeId;

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            if (!string.IsNullOrWhiteSpace(trueNodeId))
            {
                yield return trueNodeId;
            }

            if (!string.IsNullOrWhiteSpace(falseNodeId))
            {
                yield return falseNodeId;
            }
        }
    }
}
