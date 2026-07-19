namespace Verdict.Data.Narrative
{
    /// <summary>
    /// A DialogueNodeData plays a small sequence of these before advancing
    /// to its NextNodeId. Kept separate from NarrativeNodeData - this is
    /// not itself a graph node, just a step inside one.
    /// </summary>
    public enum NarrativeDialogueEntryType
    {
        Line,
        Event
    }
}
