using System;
using UnityEngine;
using Verdict.Data.Cases;

namespace Verdict.Data.Dialogue
{
    /// <summary>
    /// Connects gameplay to dialogue without either side knowing about the other.
    /// Designer drags a StatementData and a DialogueData together here -
    /// no manual ID typing, no string reference.
    /// </summary>
    [Serializable]
    public sealed class StatementDialogueBinding
    {
        [SerializeField] private StatementData statement;

        [SerializeField] private DialogueData dialogue;

        public StatementData Statement => statement;

        public DialogueData Dialogue => dialogue;

        public StatementDialogueBinding() { }

        public StatementDialogueBinding(
            StatementData statement,
            DialogueData dialogue)
        {
            this.statement = statement ?? throw new ArgumentNullException(nameof(statement));
            this.dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
        }

        public bool IsValid =>
            statement != null &&
            !string.IsNullOrWhiteSpace(statement.Id) &&
            dialogue != null;
    }
}
