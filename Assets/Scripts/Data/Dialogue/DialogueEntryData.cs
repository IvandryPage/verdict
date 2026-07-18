using System;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Data.Dialogue
{
    [Serializable]
    public sealed class DialogueEntryData
    {
        public DialogueEntryType Type;

        public DialogueLineData Line;

        public StatementData Statement;

        public DialogueEventData Event;
    }
}
