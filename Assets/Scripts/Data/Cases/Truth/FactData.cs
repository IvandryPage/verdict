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

        public FactData()
        {
        }

        public FactData(string id)
        {
            this.id = id;
        }

        public void SetSubject(FactOperand newSubject) => subject = newSubject;

        public void SetPredicate(FactPredicate newPredicate) => predicate = newPredicate;

        public void SetObject(FactOperand newObject) => @object = newObject;

        public void AddSupportingEvidence(EvidenceData evidence)
        {
            if (evidence == null)
                throw new ArgumentNullException(nameof(evidence));

            supportingEvidence.Add(evidence);
        }

        public bool RemoveSupportingEvidence(EvidenceData evidence)
        {
            if (evidence == null)
                return false;

            return supportingEvidence.Remove(evidence);
        }
    }
}
