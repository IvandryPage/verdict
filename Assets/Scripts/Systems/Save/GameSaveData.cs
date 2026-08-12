using System;
using System.Collections.Generic;

namespace Verdict.Systems.Save
{
    [Serializable]
    public sealed class StatSaveEntry
    {
        public string StatName;
        public int Value;
    }

    [Serializable]
    public sealed class StringBoolEntry
    {
        public string Key;
        public bool Value;
    }

    [Serializable]
    public sealed class GameSaveData
    {
        public string CaseId;
        public string CurrentStatementId;
        public string CurrentWitnessId;
        public string CurrentTestimonyId;
        public string CurrentNarrativeNodeId;
        public int CurrentNarrativeEntryIndex;
        public bool IsNarrativeFinished;
        public bool IsCaseFinished;

        public List<string> RevealedStatementIds = new();
        public List<string> RevealedTestimonyIds = new();
        public List<string> UnlockedEvidenceIds = new();

        public List<StatSaveEntry> CourtStats = new();
        public List<StringBoolEntry> StatementVisibility = new();
        public List<StringBoolEntry> EvidenceVisibility = new();
        public List<StringBoolEntry> EvidenceUnlocked = new();
        public List<StringBoolEntry> ClaimsResolved = new();

        public override string ToString()
        {
            return $"CaseId={CaseId}, Statement={CurrentStatementId}, Node={CurrentNarrativeNodeId}";
        }
    }
}
