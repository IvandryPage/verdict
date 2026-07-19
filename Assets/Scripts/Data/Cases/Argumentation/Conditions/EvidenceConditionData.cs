using System;
using UnityEngine;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Passes when the player's PresentEvidence argument used this
    /// specific piece of evidence.
    /// </summary>
    [Serializable]
    public sealed class EvidenceConditionData : ArgumentConditionData
    {
        [SerializeField]
        private EvidenceData requiredEvidence;

        public EvidenceConditionData()
        {
        }

        public EvidenceConditionData(EvidenceData requiredEvidence)
        {
            this.requiredEvidence = requiredEvidence;
        }

        public EvidenceData RequiredEvidence => requiredEvidence;

        public void SetRequiredEvidence(EvidenceData evidence)
        {
            requiredEvidence = evidence;
        }
    }
}
