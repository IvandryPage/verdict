using System;
using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Characters;
using Verdict.Data.Dialogue;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    [CreateAssetMenu(
        fileName = "Case_",
        menuName = "Verdict/Data/Case")]
    public class CaseData : ScriptableObject
    {
        [Header("Metadata")]
        [SerializeField] private string id;
        [SerializeField] private string title;

        [TextArea(3, 5)]
        [SerializeField] private string description;

        [SerializeField] private CaseDifficulty difficulty = CaseDifficulty.Normal;
        [SerializeField] private CasePurpose purpose = CasePurpose.Story;
        [SerializeField] private List<string> tags = new();
        [SerializeField] private int estimatedPlayTimeMinutes = 15;
        [SerializeField] private int version = 1;
        [SerializeField] private string author;

        [Header("Court Setup")]
        [SerializeField] private CourtSetupData courtSetup = new();

        [Header("Participants")]
        [SerializeField] private CharacterData judge;
        [SerializeField] private CharacterData prosecutor;
        [SerializeField] private CharacterData defenseLawyer;

        [Header("Character Overrides")]
        [SerializeField] private List<CharacterOverrideData> characterOverrides = new();

        [Header("Evidence")]
        [SerializeField] private List<EvidenceEntryData> evidence = new();

        [Header("Truth")]
        [SerializeField] private TruthData truth;

        [Header("Witnesses")]
        [SerializeField] private List<WitnessData> witnesses = new();

        [Header("Endings")]
        [SerializeField] private List<EndingData> endings = new();

        [Header("Dialogue")]
        [SerializeField]
        private CaseDialogueData dialogue = new();

        public CaseDialogueData Dialogue => dialogue;

        public string Id => id;
        public string Title => title;
        public string Description => description;
        public CaseDifficulty Difficulty => difficulty;
        public CasePurpose Purpose => purpose;
        public IReadOnlyList<string> Tags => tags;
        public int EstimatedPlayTimeMinutes => estimatedPlayTimeMinutes;
        public int Version => version;
        public string Author => author;

        public CourtSetupData CourtSetup => courtSetup;

        public CharacterData Judge => judge;
        public CharacterData Prosecutor => prosecutor;
        public CharacterData DefenseLawyer => defenseLawyer;

        public IReadOnlyList<CharacterOverrideData> CharacterOverrides => characterOverrides;

        public IReadOnlyList<EvidenceEntryData> Evidence => evidence;

        public TruthData Truth => truth;

        public IReadOnlyList<WitnessData> Witnesses => witnesses;

        public IReadOnlyList<EndingData> Endings => endings;


        public void AddWitness(
            WitnessData witness)
        {
            if (witness == null)
                throw new ArgumentNullException(nameof(witness));

            witnesses.Add(witness);
        }

        public void InsertWitness(
            int index,
            WitnessData witness)
        {
            if (witness == null)
                throw new ArgumentNullException(nameof(witness));

            witnesses.Insert(index, witness);
        }

        public bool RemoveWitness(
            WitnessData witness)
        {
            if (witness == null)
                throw new ArgumentNullException(nameof(witness));

            return witnesses.Remove(witness);
        }
    }
}
