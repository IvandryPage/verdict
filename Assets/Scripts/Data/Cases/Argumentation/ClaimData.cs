using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// An assertion about a Fact: "this fact is true" or "this fact is
    /// false" (a lie/contradiction, since every FactData listed in
    /// TruthData is by definition objectively true). A Claim owns one or
    /// more ArgumentRules describing which player arguments can resolve
    /// it and under what conditions.
    /// </summary>
    [Serializable]
    public class ClaimData
    {
        [SerializeField] private string id;

        [SerializeField] private string factId;

        [Tooltip("True if the witness claims the fact is true. False if the witness denies it (a lie/contradiction).")]
        [SerializeField] private bool isTrue = true;

        // Field name kept as "evaluationRules" for save-data compatibility
        // with already-authored cases - only the public API is renamed.
        [SerializeField] private List<ArgumentRuleData> evaluationRules = new();

        public string Id => id;

        public string FactId => factId;

        public bool IsTrue => isTrue;

        public IReadOnlyList<ArgumentRuleData> ArgumentRules => evaluationRules;

        public ClaimData() { }

        public ClaimData(string id) { this.id = id; }

        public void SetFactId(string newFactId)
        {
            factId = newFactId;
        }

        public void SetIsTrue(bool value)
        {
            isTrue = value;
        }

        public void AddArgumentRule(
            ArgumentRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            evaluationRules.Add(rule);
        }

        public void InsertArgumentRule(
            int index,
            ArgumentRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (index < 0 || index > evaluationRules.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            evaluationRules.Insert(index, rule);
        }

        public bool RemoveArgumentRule(
            ArgumentRuleData rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            return evaluationRules.Remove(rule);
        }

        public void ClearArgumentRules()
        {
            evaluationRules.Clear();
        }

        public void MoveArgumentRule(
            int oldIndex,
            int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= evaluationRules.Count)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));

            if (newIndex < 0 || newIndex >= evaluationRules.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            ArgumentRuleData rule =
                evaluationRules[oldIndex];

            evaluationRules.RemoveAt(oldIndex);

            evaluationRules.Insert(newIndex, rule);
        }
    }
}
