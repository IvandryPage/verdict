using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Generic extensibility point for custom, non-dialogue gameplay hooks
    /// (camera cues, timelines, sound triggers, future minigame modules,
    /// etc). Fires GameplayEventId and immediately continues to
    /// NextNodeId - it does not pause on its own. A specific future
    /// module can listen for its GameplayEventId and pause externally if
    /// it needs to.
    /// </summary>
    [Serializable]
    public sealed class GameplayNodeData : NarrativeNodeData
    {
        [SerializeField]
        private string gameplayEventId;

        [SerializeField]
        private string nextNodeId;

        public string GameplayEventId => gameplayEventId;

        public string NextNodeId => nextNodeId;

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            if (!string.IsNullOrWhiteSpace(nextNodeId))
            {
                yield return nextNodeId;
            }
        }
    }
}
