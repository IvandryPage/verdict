namespace Verdict.Data.Cases
{
    public enum CourtStateEffect
    {
        None,
        RevealNewStatement,
        RevealNewTestimony,
        UnlockEvidence,
        IncreaseTrust,
        DecreaseTrust,
        IncreasePenalty,
        DecreasePenalty,
        TriggerEnding
    }
}
