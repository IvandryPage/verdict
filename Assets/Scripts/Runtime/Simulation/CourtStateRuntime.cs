using System;
using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class CourtStateRuntime
    {
        public event Action<CourtStat, int, int> StatChanged;

        private readonly HashSet<string> revealedStatementIds = new();
        private readonly HashSet<string> revealedTestimonyIds = new();
        private readonly HashSet<string> unlockedEvidenceIds = new();

        // Designer-facing court stats stored in a dictionary for easy extension.
        private readonly Dictionary<CourtStat, int> courtStats = new()
        {
            { CourtStat.JudgeTrust, 70 },
            { CourtStat.Penalty, 0 },
            { CourtStat.PublicOpinion, 50 },
            { CourtStat.StoryProgress, 0 },
            { CourtStat.CaseProgress, 0 },
            { CourtStat.JuryTrust, 50 },
            { CourtStat.DefenseConfidence, 50 },
            { CourtStat.ProsecutorPressure, 50 }
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
            EnsureStatExists(stat);
            return courtStats[stat];
        }

        public void ModifyCourtStat(CourtStat stat, int value, StatOperation operation = StatOperation.Add)
        {
            EnsureStatExists(stat);

            if (operation == StatOperation.Add && value == 0)
            {
                return;
            }

            int current = GetCourtStat(stat);
            long next = current;

            switch (operation)
            {
                case StatOperation.Set:
                    next = value;
                    break;
                case StatOperation.Add:
                    next = (long)current + value;
                    break;
                case StatOperation.Subtract:
                    next = (long)current - value;
                    break;
                case StatOperation.Multiply:
                    next = (long)current * value;
                    break;
                case StatOperation.Divide:
                    if (value == 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            "Division by zero is not allowed for stat operations.");
                    }

                    next = current / value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(operation),
                        operation,
                        null);
            }

            if (next < 0)
            {
                next = 0;
            }

            int previousValue = courtStats[stat];
            int newValue = (int)next;
            courtStats[stat] = newValue;

            if (previousValue != newValue)
            {
                StatChanged?.Invoke(stat, previousValue, newValue);
            }
        }

        public void EnsureStatExists(CourtStat stat)
        {
            if (!courtStats.ContainsKey(stat))
            {
                courtStats[stat] = 0;
            }
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
