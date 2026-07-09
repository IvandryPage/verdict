using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Cases
{
    [Serializable]
    public class TestimonyData
    {
        [SerializeField] private string id;

        [SerializeField] private string title;

        [TextArea(2, 4)]
        [SerializeField] private string description;

        [SerializeField] private List<StatementData> statements = new();

        public string Id => id;

        public string Title => title;

        public string Description => description;

        public IReadOnlyList<StatementData> Statements => statements;
    }
}
