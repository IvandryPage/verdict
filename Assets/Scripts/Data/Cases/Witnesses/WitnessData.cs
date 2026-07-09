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

        [Header("Testimonies")]
        [SerializeField] private List<TestimonyData> testimonies = new();

        public string Id => id;

        public CharacterData Character => character;

        public WitnessRole Role => role;

        public IReadOnlyList<TestimonyData> Testimonies => testimonies;
    }
}
