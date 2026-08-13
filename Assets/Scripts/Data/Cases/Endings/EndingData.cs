using System;
using UnityEngine;
using Verdict.Runtime;

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

        // --- Added: one threshold per remaining CourtStat, so an ending can be
        // evaluated purely from CourtSetupData/runtime stats without needing to
        // walk the narrative graph. "minimum" is used for stats where higher is
        // better for the defense; "maximum" for stats where lower is better. ---

        [Min(0)]
        [SerializeField] private int minimumPublicOpinion;

        [Min(0)]
        [SerializeField] private int minimumCaseProgress;

        [Min(0)]
        [SerializeField] private int minimumStoryProgress;

        [Min(0)]
        [SerializeField] private int minimumJuryTrust;

        [Min(0)]
        [SerializeField] private int minimumDefenseConfidence;

        [Min(0)]
        [SerializeField] private int maximumProsecutorPressure;

        public string Id => id;

        public string Title => title;

        public string Description => description;

        public int MinimumJudgeTrust => minimumJudgeTrust;

        public int MaximumPenalty => maximumPenalty;

        public int MinimumPublicOpinion => minimumPublicOpinion;

        public int MinimumCaseProgress => minimumCaseProgress;

        public int MinimumStoryProgress => minimumStoryProgress;

        public int MinimumJuryTrust => minimumJuryTrust;

        public int MinimumDefenseConfidence => minimumDefenseConfidence;

        public int MaximumProsecutorPressure => maximumProsecutorPressure;

        /// <summary>
        /// Returns true if the given court stats satisfy every threshold on this
        /// ending. Useful for a simple resolver: sort your endings from strictest
        /// to loosest and return the first one where this is true.
        /// </summary>
        public bool IsSatisfiedBy(
            int judgeTrust, int penalty, int publicOpinion, int caseProgress,
            int storyProgress, int juryTrust, int defenseConfidence, int prosecutorPressure)
        {
            return judgeTrust >= minimumJudgeTrust
                && penalty <= maximumPenalty
                && publicOpinion >= minimumPublicOpinion
                && caseProgress >= minimumCaseProgress
                && storyProgress >= minimumStoryProgress
                && juryTrust >= minimumJuryTrust
                && defenseConfidence >= minimumDefenseConfidence
                && prosecutorPressure <= maximumProsecutorPressure;
        }

        public bool IsSatisfiedBy(CourtStateRuntime runtime)
        {
            if (runtime == null)
            {
                return false;
            }

            return IsSatisfiedBy(
                runtime.GetCourtStat(CourtStat.JudgeTrust),
                runtime.GetCourtStat(CourtStat.Penalty),
                runtime.GetCourtStat(CourtStat.PublicOpinion),
                runtime.GetCourtStat(CourtStat.CaseProgress),
                runtime.GetCourtStat(CourtStat.StoryProgress),
                runtime.GetCourtStat(CourtStat.JuryTrust),
                runtime.GetCourtStat(CourtStat.DefenseConfidence),
                runtime.GetCourtStat(CourtStat.ProsecutorPressure));
        }

        public int GetScore(CourtStateRuntime runtime)
        {
            if (runtime == null)
            {
                return int.MinValue;
            }

            int score = 0;
            score += runtime.GetCourtStat(CourtStat.JudgeTrust);
            score += runtime.GetCourtStat(CourtStat.PublicOpinion);
            score += runtime.GetCourtStat(CourtStat.JuryTrust);
            score += runtime.GetCourtStat(CourtStat.DefenseConfidence);
            score += runtime.GetCourtStat(CourtStat.CaseProgress);
            score += runtime.GetCourtStat(CourtStat.StoryProgress);
            score -= runtime.GetCourtStat(CourtStat.Penalty);
            score -= runtime.GetCourtStat(CourtStat.ProsecutorPressure);
            return score;
        }
    }
}
