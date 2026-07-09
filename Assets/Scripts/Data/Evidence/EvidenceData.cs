using UnityEngine;

namespace Verdict.Data.Evidence
{
    [CreateAssetMenu(
        fileName = "Evidence_",
        menuName = "Verdict/Data/Evidence")]
    public class EvidenceData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Description")]
        [SerializeField]
        [TextArea(3, 5)]
        private string description;

        [Header("Classification")]
        [SerializeField] private EvidenceType type;

        [Header("Visuals")]
        [SerializeField] private Sprite icon; // art in the inventory
        [SerializeField] private Sprite artwork; // closer look art

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public EvidenceType Type => type;
        public Sprite Icon => icon;
        public Sprite Artwork => artwork;
    }
}
