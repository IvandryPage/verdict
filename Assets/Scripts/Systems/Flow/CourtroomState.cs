namespace Verdict.Systems
{
    public enum CourtroomState
    {
        None,

        Statement,

        Pressing,
        Questioning,

        EvidenceSelection,
        EvidenceInspection,
        EvidencePresentation,

        Evaluating,
        Result,

        Ending,
        Paused,
    }
}
