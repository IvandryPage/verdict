using System;
using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class EvaluationRuleData
    {
        [Header("Description")]
        [TextArea(2, 3)]
        [SerializeField]
        [Tooltip("Designer notes explaining what this evaluation rule represents (e.g., 'Present CCTV footage').")]
        private string description;

        [SerializeField] private EvaluationType evaluationType;

        [SerializeField]
        [Tooltip("Required only when EvaluationType is PresentEvidence.")]
        private EvidenceData requiredEvidence;

        [SerializeField] private List<CourtStateEffectData> successEffects = new();

        [SerializeField] private List<CourtStateEffectData> failureEffects = new();

        public string Description => description;

        public EvaluationType EvaluationType => evaluationType;

        public EvidenceData RequiredEvidence => requiredEvidence;

        public IReadOnlyList<CourtStateEffectData> SuccessEffects => successEffects;

        public IReadOnlyList<CourtStateEffectData> FailureEffects => failureEffects;
    }
}
