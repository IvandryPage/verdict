using System;
using System.Collections.Generic;

namespace Verdict.Runtime
{
    public sealed class CourtStateRuntime
    {
        private readonly HashSet<string> revealedStatementIds = new();
        private readonly HashSet<string> revealedTestimonyIds = new();
        private readonly HashSet<string> unlockedEvidenceIds = new();

        public int Penalty { get; private set; }

        public int JudgeTrust { get; private set; } = 100;

        public IReadOnlyCollection<string> RevealedStatementIds => revealedStatementIds;

        public IReadOnlyCollection<string> RevealedTestimonyIds => revealedTestimonyIds;

        public IReadOnlyCollection<string> UnlockedEvidenceIds => unlockedEvidenceIds;

        public void IncreasePenalty(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            Penalty += value;
        }

        public void DecreasePenalty(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            Penalty = Math.Max(0, Penalty - value);
        }

        public void IncreaseTrust(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            JudgeTrust += value;
        }

        public void DecreaseTrust(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            JudgeTrust = Math.Max(0, JudgeTrust - value);
        }

        public bool RevealStatement(string statementId)
        {
            return revealedStatementIds.Add(statementId);
        }

        public bool RevealTestimony(string testimonyId)
        {
            return revealedTestimonyIds.Add(testimonyId);
        }

        public bool UnlockEvidence(string evidenceId)
        {
            return unlockedEvidenceIds.Add(evidenceId);
        }
    }
}
