using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class StatementData
    {
        [SerializeField] private string id;

        [TextArea(2, 5)]
        [SerializeField] private string text;

        [SerializeField]
        private bool initiallyVisible = true;

        [SerializeField]
        [Tooltip("Optional ID of the next statement to jump to after this one. If empty, flow advances to the next visible statement.")]
        private string nextStatementId;

        [SerializeField] private List<ClaimData> claims = new();

        [Header("Designer Notes")]
        [TextArea(2, 3)]
        [SerializeField]
        private string designerNotes;

        public string Id => id;

        public string Text => text;

        public bool InitiallyVisible => initiallyVisible;

        public string NextStatementId => nextStatementId;

        public IReadOnlyList<ClaimData> Claims => claims;

        public string DesignerNotes => designerNotes;
    }
}
