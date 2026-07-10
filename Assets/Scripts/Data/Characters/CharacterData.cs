using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Data.Characters
{
    [CreateAssetMenu(
        fileName = "Character_",
        menuName = "Verdict/Data/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Profile")]
        [SerializeField] private CharacterOccupation occupation;
        [SerializeField]
        [TextArea]
        private string biography;

        [Header("Default Stats")]
        [Range(0, 100)]
        [SerializeField] private int defaultCredibility = 100;

        [Range(0, 100)]
        [SerializeField] private int defaultTrust = 50;

        [Range(0, 100)]
        [SerializeField] private int defaultStress = 20;

        [Range(0, 100)]
        [SerializeField] private int defaultFear = 10;

        [Range(0, 100)]
        [SerializeField] private int defaultCooperation = 60;

        [Range(-100, 100)]
        [SerializeField] private int defaultAffinity = 0;

        // TODO: Confirm whether it uses 2d Portrait or replaces it entirely with 3D
        [Header("Visuals")]
        [SerializeField] private List<PortraitEntry> portraits = new();

        public string Id => id;
        public string DisplayName => displayName;
        public CharacterOccupation Occupation => occupation;
        public string Biography => biography;

        public int DefaultCredibility => defaultCredibility;
        public int DefaultTrust => defaultTrust;
        public int DefaultStress => defaultStress;
        public int DefaultFear => defaultFear;
        public int DefaultCooperation => defaultCooperation;
        public int DefaultAffinity => defaultAffinity;

        public IReadOnlyList<PortraitEntry> Portraits => portraits;
    }
}
