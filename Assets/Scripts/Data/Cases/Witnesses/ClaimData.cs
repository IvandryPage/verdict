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

        [SerializeField]
        [Range(0, 100)]
        [Tooltip("Optional priority for tie-breaking when multiple claims have the same evaluation result.")]
        private int priority = 0;

        [SerializeField] private List<EvaluationRuleData> evaluationRules = new();

        [Header("Designer Notes")]
        [TextArea(2, 3)]
        [SerializeField]
        private string designerNotes;

        public string Id => id;

        public string FactId => factId;

        public bool IsTrue => isTrue;

        public int Priority => priority;

        public IReadOnlyList<EvaluationRuleData> EvaluationRules => evaluationRules;

        public string DesignerNotes => designerNotes;
    }
}
