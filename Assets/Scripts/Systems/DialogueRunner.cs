using System;
using System.Diagnostics;
using Verdict.Data.Dialogue;
using Verdict.Runtime.Dialogue;

namespace Verdict.Runtime
{
    /// <summary>
    /// Executes dialogue progression.
    /// Owns no UI.
    /// Owns no gameplay.
    /// Only advances DialogueRuntime.
    /// </summary>
    public sealed class DialogueRunner
    {
        public event Action<DialogueRuntime> DialogueStarted;

        public event Action<DialogueRuntime> DialogueFinished;

        public event Action<DialogueEntryData> EntryChanged;

        public event Action<DialogueEventData> EventTriggered;

        /// <summary>
        /// Raised when the dialogue reaches a StatementMarker entry.
        /// Carries no data - the runner does not know which StatementData
        /// is active. Whoever started this dialogue already knows which
        /// statement it is bound to (via StatementDialogueBinding) and is
        /// responsible for handing control back to gameplay.
        /// </summary>
        public event Action StatementMarkerReached;

        public DialogueRuntime Runtime { get; private set; }

        public bool IsRunning =>
            Runtime != null &&
            !Runtime.IsFinished;

        public void Start(DialogueData data)
        {
            Runtime = new DialogueRuntime(data) ??
                throw new ArgumentNullException(nameof(data));

            Runtime.CurrentEntryIndex = 0;

            DialogueStarted?.Invoke(Runtime);

            ProcessCurrentEntry();
        }

        public void Continue()
        {
            if (!IsRunning)
            {
                return;
            }

            Runtime.CurrentEntryIndex++;

            if (Runtime.IsFinished)
            {
                DialogueFinished?.Invoke(Runtime);
                return;
            }

            ProcessCurrentEntry();
        }

        public void Reset()
        {
            if (Runtime == null)
            {
                return;
            }

            Runtime.CurrentEntryIndex = 0;

            ProcessCurrentEntry();
        }

        public void Stop()
        {
            Runtime = null;
        }

        private void ProcessCurrentEntry()
        {
            while (IsRunning)
            {
                DialogueEntryData entry = Runtime.CurrentEntry;

                switch (entry.Type)
                {
                    case DialogueEntryType.Line:

                        ProcessLine(entry);

                        return;

                    case DialogueEntryType.Event:

                        ProcessEvent(entry);

                        continue;

                    case DialogueEntryType.StatementMarker:

                        ProcessStatementMarker();

                        return;
                }
            }

            DialogueFinished?.Invoke(Runtime);
        }

        private void ProcessLine(DialogueEntryData entry)
        {
            EntryChanged?.Invoke(entry);
        }

        private void ProcessEvent(DialogueEntryData entry)
        {
            EventTriggered?.Invoke(entry.Event);

            Runtime.CurrentEntryIndex++;
        }

        private void ProcessStatementMarker()
        {
            StatementMarkerReached?.Invoke();
        }
    }
}
