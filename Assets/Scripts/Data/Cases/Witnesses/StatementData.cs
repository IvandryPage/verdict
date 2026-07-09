using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class StatementData
    {
        [SerializeField] private string id;

        [TextArea(2, 5)]
        [SerializeField] private string text;

        [SerializeField] private List<ClaimData> claims = new();

        public string Id => id;

        public string Text => text;

        public IReadOnlyList<ClaimData> Claims => claims;
    }
}
