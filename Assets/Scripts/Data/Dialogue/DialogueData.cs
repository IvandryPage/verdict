using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Dialogue
{
    [Serializable]
    public sealed class DialogueData
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private List<DialogueEntryData> entries = new();

        public string Id => id;

        public IReadOnlyList<DialogueEntryData> Entries => entries;
    }
}
