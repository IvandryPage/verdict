using System;
using UnityEngine;

namespace Verdict.Data.Narrative
{
    [Serializable]
    public sealed class NarrativeChoiceOptionData
    {
        [SerializeField]
        private string choiceId;

        [SerializeField]
        [TextArea(1, 3)]
        private string text;

        [SerializeField]
        private string nextNodeId;

        public NarrativeChoiceOptionData()
        {
        }

        public NarrativeChoiceOptionData(string text, string nextNodeId = null)
        {
            choiceId = Guid.NewGuid().ToString("N");
            this.text = text;
            this.nextNodeId = nextNodeId;
        }

        /// <summary>
        /// Stable per-choice id, independent of list position - used as
        /// the connection key so removing/reordering choices never
        /// silently rewires the wrong port.
        /// </summary>
        public string ChoiceId => choiceId;

        public string Text => text;

        public string NextNodeId => nextNodeId;

        public void SetText(string newText)
        {
            text = newText;
        }

        public void SetNextNodeId(string id)
        {
            nextNodeId = id;
        }
    }
}
