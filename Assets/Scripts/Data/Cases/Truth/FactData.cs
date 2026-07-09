using System;
using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class FactData
    {
        [SerializeField] private string id;

        [Header("Knowledge")]
        [SerializeField] private FactOperand subject;
        [SerializeField] private FactPredicate predicate;
        [SerializeField] private FactOperand @object;

        [Header("Supporting Evidence")]
        [SerializeField] private List<EvidenceData> supportingEvidence = new();

        public string Id => id;

        public FactOperand Subject => subject;

        public FactPredicate Predicate => predicate;

        public FactOperand Object => @object;

        public IReadOnlyList<EvidenceData> SupportingEvidence => supportingEvidence;
    }
}
