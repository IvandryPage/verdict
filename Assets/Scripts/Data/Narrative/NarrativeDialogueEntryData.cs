using System;

namespace Verdict.Data.Narrative
{
    [Serializable]
    public sealed class NarrativeDialogueEntryData
    {
        public NarrativeDialogueEntryType Type;

        public NarrativeLineData Line;

        public NarrativeEventData Event;
    }
}
