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

        public ClaimData() { }

        public ClaimData(string id) { this.id = id; }

        public void AddEvaluationRule(
            EvaluationRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            evaluationRules.Add(rule);
        }

        public void InsertEvaluationRule(
            int index,
            EvaluationRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (index < 0 || index > evaluationRules.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            evaluationRules.Insert(index, rule);
        }

        public bool RemoveEvaluationRule(
            EvaluationRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            return evaluationRules.Remove(rule);
        }

        public void ClearEvaluationRules()
        {
            evaluationRules.Clear();
        }

        public void MoveEvaluationRule(
            int oldIndex,
            int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= evaluationRules.Count)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));

            if (newIndex < 0 || newIndex >= evaluationRules.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            EvaluationRuleData rule =
                evaluationRules[oldIndex];

            evaluationRules.RemoveAt(oldIndex);

            evaluationRules.Insert(newIndex, rule);
        }
    }
}
