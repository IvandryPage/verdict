namespace Verdict.Editor.NarrativeEditor
{
    /// <summary>
    /// Identifies which outgoing slot of a node an edge belongs to.
    /// For Dialogue/Statement/Jump/Gameplay nodes (single output) this is
    /// always Next. Condition nodes use True/False. Choice nodes use the
    /// choice's own stable ChoiceId as the key.
    /// </summary>
    public static class NarrativeOutputKeys
    {
        public const string Next = "next";
        public const string ConditionTrue = "true";
        public const string ConditionFalse = "false";
    }
}
