using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    public enum ClaimRequiredState
    {
        ResolvedSuccessfully,
        ResolvedAsFailed,
        NotResolved,
        Attempted,
        NotAttempted
    }

    /// <summary>
    /// Passes when another claim (by id) is in a required state - e.g.
    /// "the alibi claim must already be resolved successfully before
    /// this contradiction becomes provable". Enables sequential
    /// contradictions without hardcoded case-specific logic.
    /// </summary>
    [Serializable]
    public sealed class ClaimConditionData : ArgumentConditionData
    {
        [SerializeField]
        private string claimId;

        [SerializeField]
        private ClaimRequiredState requiredState = ClaimRequiredState.ResolvedSuccessfully;

        public ClaimConditionData()
        {
        }

        public ClaimConditionData(string claimId, ClaimRequiredState requiredState)
        {
            this.claimId = claimId;
            this.requiredState = requiredState;
        }

        public string ClaimId => claimId;

        public ClaimRequiredState RequiredState => requiredState;

        public void SetClaimId(string id) => claimId = id;

        public void SetRequiredState(ClaimRequiredState state) => requiredState = state;
    }
}
