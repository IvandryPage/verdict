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

        public void AddFact(FactData fact)
        {
            if (fact == null)
                throw new ArgumentNullException(nameof(fact));

            facts.Add(fact);
        }

        public bool RemoveFact(FactData fact)
        {
            if (fact == null)
                return false;

            return facts.Remove(fact);
        }
    }
}
