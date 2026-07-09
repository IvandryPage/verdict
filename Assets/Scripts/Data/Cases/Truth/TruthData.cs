using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class TruthData
    {
        [SerializeField]
        private List<FactData> facts = new();

        public IReadOnlyList<FactData> Facts => facts;
    }
}
