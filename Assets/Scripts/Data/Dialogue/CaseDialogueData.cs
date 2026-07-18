using System.Collections.Generic;
using UnityEngine;

namespace Verdict.Data.Dialogue
{
    /// <summary>
    /// All dialogue belonging to a single case (Court Intro, Witness Intro,
    /// Cross Examination, Ending, Verdict, etc), plus the bindings that
    /// connect individual dialogues to gameplay statements.
    ///
    /// This is the "Dialogue Database" for a case. Gameplay (Case/Witness/
    /// Testimony/Statement) does not reach into this directly - runtime
    /// lookups are built once by RuntimeFactory.
    /// </summary>
    [System.Serializable]
    public sealed class CaseDialogueData
    {
        [SerializeField] private List<DialogueData> dialogues = new();

        [SerializeField] private List<StatementDialogueBinding> bindings = new();

        public IReadOnlyList<DialogueData> Dialogues => dialogues;

        public IReadOnlyList<StatementDialogueBinding> Bindings => bindings;
    }
}
