using System;

namespace Verdict.Data.Narrative
{
    [Serializable]
    public sealed class NarrativeEventData
    {
        public NarrativeEventType Type;

        public string Parameter;
    }
}
