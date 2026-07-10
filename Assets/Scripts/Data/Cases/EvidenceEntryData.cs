using System;
using UnityEngine;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Defines evidence availability within a case context.
    /// Each evidence entry specifies whether it should be unlocked at the start of the case.
    /// </summary>
    [Serializable]
    public class EvidenceEntryData
    {
        [SerializeField] private EvidenceData evidence;

        [SerializeField]
        [Tooltip("Can the player inspect this evidence immediately at the start of the case?")]
        private bool initiallyUnlocked = true;

        [SerializeField]
        [Tooltip("Can this evidence be presented in court?")]
        private bool canPresent = true;

        public EvidenceData Evidence => evidence;
        public bool InitiallyUnlocked => initiallyUnlocked;
        public bool CanPresent => canPresent;
    }
}
