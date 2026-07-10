using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public sealed class EndingData
    {
        [SerializeField] private string id;

        [SerializeField] private string title;

        [TextArea(3, 5)]
        [SerializeField] private string description;

        [Header("Conditions")]
        [Min(0)]
        [SerializeField] private int minimumJudgeTrust;

        [Min(0)]
        [SerializeField] private int maximumPenalty;

        [Range(0, 100)]
        [SerializeField]
        [Tooltip("Priority for tie-breaking when multiple endings meet their conditions. Higher priority = preferred.")]
        private int priority = 50;

        public string Id => id;

        public string Title => title;

        public string Description => description;

        public int MinimumJudgeTrust => minimumJudgeTrust;

        public int MaximumPenalty => maximumPenalty;

        public int Priority => priority;
    }
}
