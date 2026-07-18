using System;

namespace Verdict.Data.Dialogue
{
    [Serializable]
    public sealed class DialogueEventData
    {
        public DialogueEventType Type;

        public string Parameter;
    }
}
