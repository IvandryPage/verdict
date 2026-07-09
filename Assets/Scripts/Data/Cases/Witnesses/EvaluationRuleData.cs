using System;
using UnityEngine;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class EvaluationRuleData
    {
        [SerializeField] private EvaluationType evaluationType;

        [SerializeField] private EvidenceData requiredEvidence;

        [Tooltip("Applied when the player satisfies this rule.")]
        [SerializeField] private CourtStateEffect successEffect;

        [Tooltip("Applied when the player fails this rule.")]
        [SerializeField] private CourtStateEffect failureEffect;

        public EvaluationType EvaluationType => evaluationType;

        public EvidenceData RequiredEvidence => requiredEvidence;

        public CourtStateEffect SuccessEffect => successEffect;

        public CourtStateEffect FailureEffect => failureEffect;
    }
}
