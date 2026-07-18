using System;
using Verdict.Data.Dialogue;
using Verdict.Runtime;
using Verdict.Runtime.Dialogue;

namespace Verdict.Systems
{
    public sealed class DialogueCoordinator
    {
        private readonly DialogueRunner runner;

        public DialogueCoordinator(DialogueRunner runner)
        {
            this.runner = runner
                ?? throw new ArgumentNullException(nameof(runner));

            runner.DialogueStarted += HandleDialogueStarted;
            runner.DialogueFinished += HandleDialogueFinished;
            runner.StatementMarkerReached += HandleStatementReached;
        }

        public bool IsPlaying => runner.IsRunning;

        public bool CanResume =>
            runner.IsRunning &&
            runner.Runtime != null &&
            runner.Runtime.CurrentEntryType == DialogueEntryType.StatementMarker;

        public event Action DialogueStarted;

        public event Action DialogueFinished;

        public event Action StatementReached;


        public void Play(DialogueData dialogue)
        {
            if (dialogue == null)
                return;

            runner.Start(dialogue);
        }


        public void Resume()
        {
            if (!CanResume)
                return;

            runner.Continue();
        }

        public void Stop()
        {
            if (!runner.IsRunning)
                return;

            runner.Stop();
        }

        private void HandleDialogueStarted(DialogueRuntime runtime)
        {
            DialogueStarted?.Invoke();
        }

        private void HandleDialogueFinished(DialogueRuntime runtime)
        {
            DialogueFinished?.Invoke();
        }

        private void HandleStatementReached()
        {
            StatementReached?.Invoke();
        }
    }
}
