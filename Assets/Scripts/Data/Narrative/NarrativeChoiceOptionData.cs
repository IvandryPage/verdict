using System;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    [Serializable]
    public sealed class NarrativeChoiceOptionData
    {
        [SerializeField]
        [TextArea(1, 3)]
        private string text;

        [SerializeField]
        private string nextNodeId;

        public string Text => text;

        public string NextNodeId => nextNodeId;
    }
}
