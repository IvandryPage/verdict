using System;
using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class EvaluationRuleData
    {
        [SerializeField] private EvaluationType evaluationType;

        [SerializeField]
        [Tooltip("Required only when EvaluationType is PresentEvidence.")]
        private EvidenceData requiredEvidence;

        [SerializeField] private List<CourtStateEffectData> successEffects = new();

        [SerializeField] private List<CourtStateEffectData> failureEffects = new();

        public EvaluationType EvaluationType => evaluationType;

        public EvidenceData RequiredEvidence => requiredEvidence;

        public IReadOnlyList<CourtStateEffectData> SuccessEffects => successEffects;

        public IReadOnlyList<CourtStateEffectData> FailureEffects => failureEffects;
    }
}
