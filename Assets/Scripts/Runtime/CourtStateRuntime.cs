using System;
using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class CourtStateRuntime
    {
        private readonly HashSet<string> revealedStatementIds = new();
        private readonly HashSet<string> revealedTestimonyIds = new();
        private readonly HashSet<string> unlockedEvidenceIds = new();

        // Designer-facing court stats stored in a dictionary for easy extension.
        private readonly Dictionary<CourtStat, int> courtStats = new()
        {
            { CourtStat.JudgeTrust, 100 },
            { CourtStat.Penalty, 0 }
        };

        public IReadOnlyCollection<string> RevealedStatementIds => revealedStatementIds;

        // Persistent state only: save/load can use this list to recreate revealed statements.
        // Gameplay should rely on StatementRuntime.IsVisible instead.

        public IReadOnlyCollection<string> RevealedTestimonyIds => revealedTestimonyIds;

        public IReadOnlyCollection<string> UnlockedEvidenceIds => unlockedEvidenceIds;

        // Compatibility properties map to the dictionary-backed stats.
        public int Penalty => GetCourtStat(CourtStat.Penalty);

        public int JudgeTrust => GetCourtStat(CourtStat.JudgeTrust);

        public int GetCourtStat(CourtStat stat)
        {
            if (courtStats.TryGetValue(stat, out int value))
            {
                return value;
            }

            return 0;
        }

        public void ModifyCourtStat(CourtStat stat, int delta)
        {
            // delta can be negative to decrease.
            if (delta == 0)
            {
                return;
            }

            int current = GetCourtStat(stat);
            long next = (long)current + delta;
            if (next < 0)
            {
                next = 0;
            }

            courtStats[stat] = (int)next;
        }

        // Backwards-compatible helpers that map to the stat-based API.
        public void IncreasePenalty(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            ModifyCourtStat(CourtStat.Penalty, value);
        }

        public void DecreasePenalty(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            ModifyCourtStat(CourtStat.Penalty, -value);
        }

        public void IncreaseTrust(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            ModifyCourtStat(CourtStat.JudgeTrust, value);
        }

        public void DecreaseTrust(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            ModifyCourtStat(CourtStat.JudgeTrust, -value);
        }

        public bool RevealStatement(string statementId)
        {
            return revealedStatementIds.Add(
                ValidateTargetId(statementId));
        }

        public bool RevealTestimony(string testimonyId)
        {
            return revealedTestimonyIds.Add(
                ValidateTargetId(testimonyId));
        }

        public bool UnlockEvidence(string evidenceId)
        {
            return unlockedEvidenceIds.Add(
                ValidateTargetId(evidenceId));
        }

        private static string ValidateTargetId(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException(
                    "Target ID cannot be null or empty.",
                    nameof(targetId));
            }

            return targetId;
        }
    }
}
