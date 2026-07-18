using System;

namespace Verdict.Data.Dialogue
{
    /// <summary>
    /// A single entry in a dialogue sequence.
    /// Knows nothing about gameplay (Statement, Witness, Testimony, etc).
    /// A StatementMarker entry carries no payload - it is a pure positional
    /// marker telling the runner to hand control back to gameplay. Which
    /// StatementData is "current" is decided outside the dialogue layer,
    /// via StatementDialogueBinding.
    /// </summary>
    [Serializable]
    public sealed class DialogueEntryData
    {
        public DialogueEntryType Type;

        public DialogueLineData Line;

        public DialogueEventData Event;
    }
}
