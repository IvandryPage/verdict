using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Characters;
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

        [Header("Participants")]
        [SerializeField] private CharacterData judge;
        [SerializeField] private CharacterData prosecutor;
        [SerializeField] private CharacterData defenseLawyer;

        [Header("Evidence")]
        [SerializeField] private List<EvidenceData> evidence = new();

        [Header("Truth")]
        [SerializeField] private TruthData truth;

        [Header("Witnesses")]
        [SerializeField] private List<WitnessData> witnesses = new();


        [Header("Endings")]
        [SerializeField] private List<EndingData> endings = new();

        public string Id => id;
        public string Title => title;
        public string Description => description;

        public CharacterData Judge => judge;
        public CharacterData Prosecutor => prosecutor;
        public CharacterData DefenseLawyer => defenseLawyer;

        public IReadOnlyList<EvidenceData> Evidence => evidence;

        public TruthData Truth => truth;

        public IReadOnlyList<WitnessData> Witnesses => witnesses;

        public IReadOnlyList<EndingData> Endings => endings;

    }
}
