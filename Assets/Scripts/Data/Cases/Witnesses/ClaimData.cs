using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class ClaimData
    {
        [SerializeField] private string id;

        [SerializeField] private string factId;

        [Tooltip("True if the witness claims the fact is true. False if the witness denies it.")]
        [SerializeField] private bool isTrue = true;

        [SerializeField] private List<EvaluationRuleData> evaluationRules = new();

        public string Id => id;

        public string FactId => factId;

        public bool IsTrue => isTrue;

        public IReadOnlyList<EvaluationRuleData> EvaluationRules => evaluationRules;
    }
}
