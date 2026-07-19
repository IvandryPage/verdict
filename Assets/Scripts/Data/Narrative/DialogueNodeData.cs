using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Plays a short sequence of dialogue entries, then advances to
    /// NextNodeId. Contains dialogue content. Nothing else - no gameplay,
    /// no reference to Statement.
    /// </summary>
    [Serializable]
    public sealed class DialogueNodeData : NarrativeNodeData
    {
        [SerializeField]
        private List<NarrativeDialogueEntryData> entries = new();

        [SerializeField]
        private string nextNodeId;

        public IReadOnlyList<NarrativeDialogueEntryData> Entries => entries;

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
