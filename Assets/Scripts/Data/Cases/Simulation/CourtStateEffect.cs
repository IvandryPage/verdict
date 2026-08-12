namespace Verdict.Data.Cases
{
    public enum CourtStateEffect
    {
        None,

        // Flow effects (control progression)
        RevealStatement,
        RevealTestimony,
        RevealWitness,

        UnlockEvidence,

        JumpStatement,
        JumpTestimony,
        JumpWitness,

        // Generic stat modification
        ModifyCourtStat,

        // Character-scoped stat modification
        ModifyCharacterStat,

        TriggerEnding
    }
}
