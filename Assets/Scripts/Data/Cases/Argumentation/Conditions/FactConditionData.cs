using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Passes when the evidence presented by the player is one of the
    /// SupportingEvidence entries for the referenced Fact - i.e. the
    /// player just proved that fact. This is the core "present the right
    /// proof for the truth" mechanic, decoupled from any single Claim.
    /// </summary>
    [Serializable]
    public sealed class FactConditionData : ArgumentConditionData
    {
        [SerializeField]
        private string factId;

        [SerializeField]
        [Tooltip("If true, the presented evidence must be listed as supporting evidence for this fact.")]
        private bool requireSupportingEvidencePresented = true;

        public FactConditionData()
        {
        }

        public FactConditionData(string factId, bool requireSupportingEvidencePresented = true)
        {
            this.factId = factId;
            this.requireSupportingEvidencePresented = requireSupportingEvidencePresented;
        }

        public string FactId => factId;

        public bool RequireSupportingEvidencePresented => requireSupportingEvidencePresented;

        public void SetFactId(string id)
        {
            factId = id;
        }

        public void SetRequireSupportingEvidencePresented(bool value)
        {
            requireSupportingEvidencePresented = value;
        }
    }
}
