using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Runtime;

namespace Verdict.Systems
{
    /// <summary>
    /// Drives courtroom progression using runtime-visible statements.
    ///
    /// Rules:
    /// - StatementData.InitiallyVisible controls initial runtime availability.
    /// - RevealNewStatement effects set StatementRuntime.IsVisible directly.
    /// - StatementData.NextStatementId defines an explicit next target when present.
    /// - GoToStatement/TryMoveToStatement are explicit engine-controlled jumps.
    ///
    /// Designer guidance:
    /// - Use initiallyVisible when a statement should be hidden until revealed.
    /// - Use nextStatementId to make a statement override the default visible successor.
    /// - Use GoToStatement only for engine/controller-directed navigation.
    ///
    /// Gameplay must not derive visibility from CourtStateRuntime.RevealedStatementIds.
    /// That collection is for persistence/save only.
    /// </summary>
    public sealed class CourtroomFlow
    {
        private readonly CaseRuntime runtime;
        private readonly Dictionary<string, StatementLocation> statementLocations;
        private readonly Dictionary<string, (int WitnessIndex, int TestimonyIndex)> testimonyLocationIndices;
        private readonly Dictionary<string, int> witnessLocationIndices;

        private int witnessIndex;
        private int testimonyIndex;
        private int statementIndex;

        public CourtroomFlow(CaseRuntime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            statementLocations = BuildStatementLocations(
                runtime,
                out testimonyLocationIndices,
                out witnessLocationIndices);
            Reset();
        }

        public CaseRuntime Runtime => runtime;

        public WitnessRuntime CurrentWitness =>
            runtime.Witnesses[witnessIndex];

        public TestimonyRuntime CurrentTestimony =>
            CurrentWitness.Testimonies[testimonyIndex];

        public StatementRuntime CurrentStatement =>
            CurrentTestimony.Statements[statementIndex];

        public bool IsFirstWitness => witnessIndex == 0;

        public bool IsLastWitness =>
            FindNextVisibleStatementInWitness(witnessIndex + 1) == null;

        public bool IsFirstTestimony => testimonyIndex == 0;

        public bool IsLastTestimony =>
            FindNextVisibleStatementInTestimony(witnessIndex, testimonyIndex + 1) == null;

        public bool IsFirstStatement =>
            FindPreviousVisibleStatementInCurrentTestimony() == null;

        public bool IsLastStatement =>
            FindNextVisibleStatementInCurrentTestimony() == null &&
            !HasExplicitVisibleNextStatement();

        /// <summary>
        /// True when the flow has a visible successor from the current statement,
        /// either the next visible in-sequence or an explicit nextStatementId target.
        /// </summary>
        public bool CanMoveNextStatement =>
            FindNextVisibleStatementInCurrentTestimony() != null ||
            HasExplicitVisibleNextStatement();

        /// <summary>
        /// True when there is a visible predecessor to the current statement.
        /// </summary>
        public bool CanMovePreviousStatement => FindPreviousVisibleStatementInCurrentTestimony() != null;

        public bool CanMoveNextTestimony => FindNextVisibleStatementInTestimony(witnessIndex, testimonyIndex + 1) != null;

        public bool CanMovePreviousTestimony => FindPreviousVisibleStatementInPreviousTestimony() != null;

        public bool CanMoveNextWitness => FindNextVisibleStatementInWitness(witnessIndex + 1) != null;

        public bool CanMovePreviousWitness => FindPreviousVisibleStatementInPreviousWitness() != null;

        public bool IsComplete => !HasNextVisibleStatement();

        public bool MoveNextStatement()
        {
            StatementLocation? nextLocation = FindNextVisibleStatementInCurrentTestimony();
            if (!nextLocation.HasValue)
            {
                return false;
            }

            SetCurrentLocation(nextLocation.Value);
            return true;
        }

        public bool MovePreviousStatement()
        {
            StatementLocation? previousLocation = FindPreviousVisibleStatementInCurrentTestimony();
            if (!previousLocation.HasValue)
            {
                return false;
            }

            SetCurrentLocation(previousLocation.Value);
            return true;
        }

        public bool MoveNextTestimony()
        {
            StatementLocation? nextLocation = FindNextVisibleStatementInTestimony(witnessIndex, testimonyIndex + 1);
            if (!nextLocation.HasValue)
            {
                return false;
            }

            SetCurrentLocation(nextLocation.Value);
            return true;
        }

        public bool MovePreviousTestimony()
        {
            StatementLocation? previousLocation = FindPreviousVisibleStatementInPreviousTestimony();
            if (!previousLocation.HasValue)
            {
                return false;
            }

            SetCurrentLocation(previousLocation.Value);
            return true;
        }

        public bool MoveNextWitness()
        {
            StatementLocation? nextLocation = FindNextVisibleStatementInWitness(witnessIndex + 1);
            if (!nextLocation.HasValue)
            {
                return false;
            }

            SetCurrentLocation(nextLocation.Value);
            return true;
        }

        public bool MovePreviousWitness()
        {
            StatementLocation? previousLocation = FindPreviousVisibleStatementInPreviousWitness();
            if (!previousLocation.HasValue)
            {
                return false;
            }

            SetCurrentLocation(previousLocation.Value);
            return true;
        }

        public bool MoveNext()
        {
            if (TryFollowExplicitNext())
            {
                return true;
            }

            if (MoveNextStatement())
            {
                return true;
            }

            if (MoveNextTestimony())
            {
                return true;
            }

            if (MoveNextWitness())
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            witnessIndex = 0;
            testimonyIndex = 0;
            statementIndex = 0;

            StatementLocation? firstVisible = FindNextVisibleStatementFrom(0, 0, -1);
            if (firstVisible.HasValue)
            {
                SetCurrentLocation(firstVisible.Value);
            }
        }

        public bool TryMoveToStatement(string statementId)
        {
            if (!TryGetStatementLocation(statementId, out StatementLocation target) || !IsStatementVisible(target))
            {
                return false;
            }

            SetCurrentLocation(target);
            return true;
        }

        public bool TryMoveToTestimony(string testimonyId)
        {
            if (!TryGetTestimonyLocation(testimonyId, out StatementLocation target) || !IsStatementVisible(target))
            {
                return false;
            }

            SetCurrentLocation(target);
            return true;
        }

        public void GoToTestimony(string testimonyId)
        {
            if (!TryMoveToTestimony(testimonyId))
            {
                throw new InvalidOperationException(
                    $"Testimony '{testimonyId}' cannot be reached or contains no visible statements.");
            }
        }

        public bool TryMoveToWitness(string witnessId)
        {
            if (!TryGetWitnessLocation(witnessId, out StatementLocation target) || !IsStatementVisible(target))
            {
                return false;
            }

            SetCurrentLocation(target);
            return true;
        }

        public void GoToWitness(string witnessId)
        {
            if (!TryMoveToWitness(witnessId))
            {
                throw new InvalidOperationException(
                    $"Witness '{witnessId}' cannot be reached or contains no visible statements.");
            }
        }

        public void GoToStatement(string statementId)
        {
            if (!TryMoveToStatement(statementId))
            {
                throw new InvalidOperationException(
                    $"Statement '{statementId}' cannot be reached or is not visible.");
            }
        }


        private bool TryFollowExplicitNext()
        {
            string nextStatementId = CurrentStatement.Data.NextStatementId;
            if (string.IsNullOrWhiteSpace(nextStatementId))
            {
                return false;
            }

            if (!TryGetStatementLocation(nextStatementId, out StatementLocation target) || !IsStatementVisible(target))
            {
                return false;
            }

            SetCurrentLocation(target);
            return true;
        }

        private bool HasNextVisibleStatement()
        {
            string nextStatementId = CurrentStatement.Data.NextStatementId;
            if (!string.IsNullOrWhiteSpace(nextStatementId) &&
                TryGetStatementLocation(nextStatementId, out StatementLocation target) &&
                IsStatementVisible(target))
            {
                return true;
            }

            return FindNextVisibleStatementFrom(witnessIndex, testimonyIndex, statementIndex).HasValue;
        }

        private bool HasExplicitVisibleNextStatement()
        {
            string nextStatementId = CurrentStatement.Data.NextStatementId;
            return !string.IsNullOrWhiteSpace(nextStatementId) &&
                TryGetStatementLocation(nextStatementId, out StatementLocation target) &&
                IsStatementVisible(target);
        }

        private bool TryGetStatementLocation(string statementId, out StatementLocation location)
        {
            if (string.IsNullOrWhiteSpace(statementId))
            {
                location = default;
                return false;
            }

            return statementLocations.TryGetValue(statementId, out location);
        }

        private bool TryGetTestimonyLocation(string testimonyId, out StatementLocation location)
        {
            location = default;

            if (string.IsNullOrWhiteSpace(testimonyId))
            {
                return false;
            }

            if (!testimonyLocationIndices.TryGetValue(testimonyId, out var indices))
            {
                return false;
            }

            return FindNextVisibleStatementFrom(indices.WitnessIndex, indices.TestimonyIndex, -1) is StatementLocation target
                ? AssignLocation(ref location, target)
                : false;
        }

        private bool TryGetWitnessLocation(string witnessId, out StatementLocation location)
        {
            location = default;

            if (string.IsNullOrWhiteSpace(witnessId))
            {
                return false;
            }

            if (!witnessLocationIndices.TryGetValue(witnessId, out int witnessIndex))
            {
                return false;
            }

            return FindNextVisibleStatementFrom(witnessIndex, 0, -1) is StatementLocation target
                ? AssignLocation(ref location, target)
                : false;
        }

        private static bool AssignLocation(ref StatementLocation location, StatementLocation value)
        {
            location = value;
            return true;
        }

        private StatementLocation? FindNextVisibleStatementInCurrentTestimony()
        {
            return FindNextVisibleStatementFrom(witnessIndex, testimonyIndex, statementIndex);
        }

        private StatementLocation? FindPreviousVisibleStatementInCurrentTestimony()
        {
            return FindPreviousVisibleStatementFrom(witnessIndex, testimonyIndex, statementIndex);
        }

        private StatementLocation? FindNextVisibleStatementInTestimony(int witnessIndex, int testimonyIndex)
        {
            return FindNextVisibleStatementFrom(witnessIndex, testimonyIndex, -1);
        }

        private StatementLocation? FindPreviousVisibleStatementInPreviousTestimony()
        {
            for (int witness = witnessIndex; witness >= 0; witness--)
            {
                int testimony = witness == witnessIndex ? testimonyIndex - 1 : runtime.Witnesses[witness].Testimonies.Count - 1;

                for (; testimony >= 0; testimony--)
                {
                    StatementLocation? candidate = FindLastVisibleStatementInTestimony(witness, testimony);
                    if (candidate.HasValue)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private StatementLocation? FindNextVisibleStatementInWitness(int witnessIndex)
        {
            return FindNextVisibleStatementFrom(witnessIndex, 0, -1);
        }

        private StatementLocation? FindPreviousVisibleStatementInPreviousWitness()
        {
            for (int witness = witnessIndex - 1; witness >= 0; witness--)
            {
                StatementLocation? candidate = FindLastVisibleStatementInWitness(witness);
                if (candidate.HasValue)
                {
                    return candidate;
                }
            }

            return null;
        }

        private StatementLocation? FindNextVisibleStatementFrom(int startWitness, int startTestimony, int startStatement)
        {
            for (int witness = startWitness; witness < runtime.Witnesses.Count; witness++)
            {
                IReadOnlyList<TestimonyRuntime> testimonies = runtime.Witnesses[witness].Testimonies;
                int testimonyStart = witness == startWitness ? startTestimony : 0;

                for (int testimony = testimonyStart; testimony < testimonies.Count; testimony++)
                {
                    IReadOnlyList<StatementRuntime> statements = testimonies[testimony].Statements;
                    int statementStart = (witness == startWitness && testimony == startTestimony)
                        ? startStatement + 1
                        : 0;

                    for (int statement = Math.Max(statementStart, 0); statement < statements.Count; statement++)
                    {
                        if (statements[statement].IsVisible)
                        {
                            return new StatementLocation(witness, testimony, statement);
                        }
                    }
                }
            }

            return null;
        }

        private StatementLocation? FindPreviousVisibleStatementFrom(int witnessIndex, int testimonyIndex, int statementIndex)
        {
            IReadOnlyList<StatementRuntime> statements = runtime.Witnesses[witnessIndex].Testimonies[testimonyIndex].Statements;
            for (int statement = statementIndex - 1; statement >= 0; statement--)
            {
                if (statements[statement].IsVisible)
                {
                    return new StatementLocation(witnessIndex, testimonyIndex, statement);
                }
            }

            return null;
        }

        private StatementLocation? FindLastVisibleStatementInTestimony(int witnessIndex, int testimonyIndex)
        {
            IReadOnlyList<StatementRuntime> statements = runtime.Witnesses[witnessIndex].Testimonies[testimonyIndex].Statements;
            for (int statement = statements.Count - 1; statement >= 0; statement--)
            {
                if (statements[statement].IsVisible)
                {
                    return new StatementLocation(witnessIndex, testimonyIndex, statement);
                }
            }

            return null;
        }

        private StatementLocation? FindLastVisibleStatementInWitness(int witnessIndex)
        {
            for (int testimony = runtime.Witnesses[witnessIndex].Testimonies.Count - 1; testimony >= 0; testimony--)
            {
                StatementLocation? candidate = FindLastVisibleStatementInTestimony(witnessIndex, testimony);
                if (candidate.HasValue)
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool IsStatementVisible(StatementLocation location)
        {
            return runtime.Witnesses[location.WitnessIndex]
                .Testimonies[location.TestimonyIndex]
                .Statements[location.StatementIndex]
                .IsVisible;
        }

        private void SetCurrentLocation(StatementLocation location)
        {
            witnessIndex = location.WitnessIndex;
            testimonyIndex = location.TestimonyIndex;
            statementIndex = location.StatementIndex;
        }

        private static Dictionary<string, StatementLocation> BuildStatementLocations(
            CaseRuntime runtime,
            out Dictionary<string, (int WitnessIndex, int TestimonyIndex)> testimonyLocationIndices,
            out Dictionary<string, int> witnessLocationIndices)
        {
            var locations = new Dictionary<string, StatementLocation>(StringComparer.Ordinal);
            testimonyLocationIndices = new Dictionary<string, (int WitnessIndex, int TestimonyIndex)>(StringComparer.Ordinal);
            witnessLocationIndices = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int witness = 0; witness < runtime.Witnesses.Count; witness++)
            {
                for (int testimony = 0; testimony < runtime.Witnesses[witness].Testimonies.Count; testimony++)
                {
                    string testimonyId = runtime.Witnesses[witness].Testimonies[testimony].Data.Id;
                    if (!string.IsNullOrWhiteSpace(testimonyId) && !testimonyLocationIndices.ContainsKey(testimonyId))
                    {
                        testimonyLocationIndices.Add(testimonyId, (witness, testimony));
                    }

                    if (!witnessLocationIndices.ContainsKey(runtime.Witnesses[witness].Data.Id))
                    {
                        witnessLocationIndices.Add(runtime.Witnesses[witness].Data.Id, witness);
                    }
                    for (int statement = 0; statement < runtime.Witnesses[witness].Testimonies[testimony].Statements.Count; statement++)
                    {
                        StatementRuntime statementRuntime = runtime.Witnesses[witness].Testimonies[testimony].Statements[statement];
                        string statementId = statementRuntime.Data.Id;
                        if (!string.IsNullOrWhiteSpace(statementId) && !locations.ContainsKey(statementId))
                        {
                            locations.Add(statementId, new StatementLocation(witness, testimony, statement));
                        }
                    }
                }
            }

            return locations;
        }

        private readonly struct StatementLocation
        {
            public StatementLocation(int witnessIndex, int testimonyIndex, int statementIndex)
            {
                WitnessIndex = witnessIndex;
                TestimonyIndex = testimonyIndex;
                StatementIndex = statementIndex;
            }

            public int WitnessIndex { get; }

            public int TestimonyIndex { get; }

            public int StatementIndex { get; }
        }
    }
}
