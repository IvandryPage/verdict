using System;
using UnityEngine;
using Verdict.Data.Characters;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// Allows a case to override default character stats without modifying the global CharacterData.
    /// RuntimeFactory applies these overrides after initializing character stats from CharacterData defaults.
    /// </summary>
    [Serializable]
    public class CharacterOverrideData
    {
        [SerializeField] private CharacterData character;

        [Header("Override Stats")]
        [SerializeField] private bool overrideCredibility = false;
        [SerializeField] private int credibility = 100;

        [SerializeField] private bool overrideTrust = false;
        [SerializeField] private int trust = 50;

        [SerializeField] private bool overrideStress = false;
        [SerializeField] private int stress = 20;

        [SerializeField] private bool overrideFear = false;
        [SerializeField] private int fear = 10;

        [SerializeField] private bool overrideCooperation = false;
        [SerializeField] private int cooperation = 60;

        [SerializeField] private bool overrideAffinity = false;
        [SerializeField] private int affinity = 0;

        public CharacterData Character => character;

        public bool OverrideCredibility => overrideCredibility;
        public int Credibility => credibility;

        public bool OverrideTrust => overrideTrust;
        public int Trust => trust;

        public bool OverrideStress => overrideStress;
        public int Stress => stress;

        public bool OverrideFear => overrideFear;
        public int Fear => fear;

        public bool OverrideCooperation => overrideCooperation;
        public int Cooperation => cooperation;

        public bool OverrideAffinity => overrideAffinity;
        public int Affinity => affinity;
    }
}
