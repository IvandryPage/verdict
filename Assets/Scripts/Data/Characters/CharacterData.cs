using System.Collections.Generic;
using UnityEngine;

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

        // TODO: Confirm whether it uses 2d Portrait or replaces it entirely with 3D
        [Header("Visuals")]
        [SerializeField] private List<PortraitEntry> portraits = new();

        public string Id => id;
        public string DisplayName => displayName;
        public CharacterOccupation Occupation => occupation;
        public string Biography => biography;
        public IReadOnlyList<PortraitEntry> Portraits => portraits;
    }
}
