using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Terminates narrative playback. EndingId is optional - when set it
    /// should match an EndingData.Id on the case, letting the controller
    /// raise the matching ending.
    /// </summary>
    [Serializable]
    public sealed class EndNodeData : NarrativeNodeData
    {
        [SerializeField]
        private string endingId;

        public string EndingId => endingId;

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            yield break;
        }
    }
}
