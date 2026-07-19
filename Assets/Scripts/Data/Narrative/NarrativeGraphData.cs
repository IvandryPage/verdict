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
    }
}
