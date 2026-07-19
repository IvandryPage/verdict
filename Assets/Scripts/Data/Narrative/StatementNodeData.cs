using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    /// <summary>
    /// References a gameplay StatementData by id - NOT a dialogue.
    /// When the graph reaches this node it pauses; the controller unlocks
    /// gameplay for the referenced statement, waits for evaluation, then
    /// resumes the graph at NextNodeId.
    ///
    /// StatementId is a plain id (like NextStatementId elsewhere in the
    /// codebase) rather than a direct object reference, because
    /// StatementData is a plain embedded [Serializable] class, not an
    /// asset - Unity can't maintain a live cross-reference to it.
    /// </summary>
    [Serializable]
    public sealed class StatementNodeData : NarrativeNodeData
    {
        [SerializeField]
        private string statementId;

        [SerializeField]
        private string nextNodeId;

        public string StatementId => statementId;

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
