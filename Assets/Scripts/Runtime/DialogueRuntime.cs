using Verdict.Data.Dialogue;

namespace Verdict.Runtime.Dialogue
{
    public sealed class DialogueRuntime
    {
        public DialogueRuntime(DialogueData data)
        {
            Data = data;
        }

        public DialogueData Data { get; }

        public int CurrentEntryIndex { get; internal set; } = 0;

        public bool IsFinished =>
            CurrentEntryIndex >= Data.Entries.Count;

        public int EntryCount => Data.Entries.Count;

        public bool HasNextEntry => CurrentEntryIndex + 1 < Data.Entries.Count;

        public bool HasPreviousEntry => CurrentEntryIndex > 0;

        public DialogueEntryType CurrentEntryType =>
            CurrentEntry?.Type ?? DialogueEntryType.Line;

        public DialogueEventData CurrentEntryEvent =>
            CurrentEntry?.Event;

        public DialogueEntryData CurrentEntry
        {
            get
            {
                if (CurrentEntryIndex < 0)
                    return null;

                if (IsFinished)
                    return null;

                return Data.Entries[CurrentEntryIndex];
            }
        }
    }
}
