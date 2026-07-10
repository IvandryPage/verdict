using System;
using UnityEngine;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Defines the initial court setup values for a case.
    /// These values are copied into CourtStateRuntime during game initialization.
    /// </summary>
    [Serializable]
    public class CourtSetupData
    {
        [Header("Initial Court State")]
        [Range(0, 100)]
        [SerializeField] private int judgeTrust = 70;

        [Range(0, 100)]
        [SerializeField] private int penalty = 0;

        [Range(0, 100)]
        [SerializeField] private int publicOpinion = 50;

        [Range(0, 100)]
        [SerializeField] private int juryOpinion = 50;

        [SerializeField] private int storyProgress = 0;

        [SerializeField] private int caseProgress = 0;

        public int JudgeTrust => judgeTrust;
        public int Penalty => penalty;
        public int PublicOpinion => publicOpinion;
        public int JuryOpinion => juryOpinion;
        public int StoryProgress => storyProgress;
        public int CaseProgress => caseProgress;
    }
}
