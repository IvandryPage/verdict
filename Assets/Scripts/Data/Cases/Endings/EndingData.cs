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

        public string Id => id;

        public string Title => title;

        public string Description => description;

        public int MinimumJudgeTrust => minimumJudgeTrust;

        public int MaximumPenalty => maximumPenalty;
    }
}
