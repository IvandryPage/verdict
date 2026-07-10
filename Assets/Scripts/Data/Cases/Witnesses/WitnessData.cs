using System;
using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Characters;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class WitnessData
    {
        [SerializeField] private string id;

        [Header("Identity")]
        [SerializeField] private CharacterData character;
        [SerializeField] private WitnessRole role;

        [TextArea(2, 3)]
        [SerializeField]
        private string description;

        [Header("Testimonies")]
        [SerializeField] private List<TestimonyData> testimonies = new();

        [SerializeField]
        private bool initiallyVisible = true;

        public string Id => id;

        public CharacterData Character => character;

        public WitnessRole Role => role;

        public string Description => description;

        public IReadOnlyList<TestimonyData> Testimonies => testimonies;

        public bool InitiallyVisible => initiallyVisible;
    }
}
