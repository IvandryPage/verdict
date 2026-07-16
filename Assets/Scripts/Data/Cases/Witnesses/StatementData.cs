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

        public string Id => id;

        public string Text => text;

        public bool InitiallyVisible => initiallyVisible;

        public string NextStatementId => nextStatementId;

        public IReadOnlyList<ClaimData> Claims => claims;

        public StatementData() { }

        public StatementData(
            string id,
            string text,
            bool initiallyVisible = true,
            string nextStatementId = "",
            List<ClaimData> claims = null)
        {
            this.id = id;
            this.text = text;

            this.initiallyVisible = initiallyVisible;
            this.nextStatementId = nextStatementId;

            this.claims = claims ?? new List<ClaimData>();
        }

        public void SetNextStatement(
            string id)
        {
            if (nextStatementId == id)
                return;

            nextStatementId = id;
        }

        public void AddClaim(
            ClaimData claim)
        {
            Debug.Log($"AddClaim {claim.Id}");
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            claims.Add(claim);
        }

        public void InsertClaim(
            int index,
            ClaimData claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            if (index < 0 || index > claims.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            claims.Insert(index, claim);
        }

        public bool RemoveClaim(
            ClaimData claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            return claims.Remove(claim);
        }

        public void ClearClaims()
        {
            claims.Clear();
        }

        public void MoveClaim(
            int oldIndex,
            int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= claims.Count)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));

            if (newIndex < 0 || newIndex >= claims.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            ClaimData claim = claims[oldIndex];

            claims.RemoveAt(oldIndex);

            claims.Insert(newIndex, claim);
        }
    }
}
