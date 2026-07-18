using System;
using Verdict.Data.Cases;
using Verdict.Data.Dialogue;

namespace Verdict.Runtime
{
    public sealed class StatementRuntime
    {
        public StatementRuntime(
            StatementData data,
            DialogueData dialogue = null)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Dialogue = dialogue;
        }

        public StatementData Data { get; }

        /// <summary>
        /// Dialogue bound to this statement via StatementDialogueBinding, if any.
        /// Not every statement has one.
        /// </summary>
        public DialogueData Dialogue { get; }

        public bool HasDialogue => Dialogue != null;

        public bool IsVisible { get; set; }

        public bool HasBeenVisited { get; set; }

        public bool HasBeenPressed { get; set; }

        public bool HasBeenResolved { get; set; }
    }
}
