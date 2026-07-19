using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// Presents choices to the player. Pauses the graph until a choice
    /// is selected, then advances to that choice's NextNodeId.
    /// </summary>
    [Serializable]
    public sealed class ChoiceNodeData : NarrativeNodeData
    {
        [SerializeField]
        private List<NarrativeChoiceOptionData> choices = new();

        public IReadOnlyList<NarrativeChoiceOptionData> Choices => choices;

        public override IEnumerable<string> GetOutgoingNodeIds()
        {
            return choices
                .Where(choice => !string.IsNullOrWhiteSpace(choice.NextNodeId))
                .Select(choice => choice.NextNodeId);
        }
    }
}
