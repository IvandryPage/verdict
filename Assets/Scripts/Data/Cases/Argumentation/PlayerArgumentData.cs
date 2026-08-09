using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// A single argument the player makes during courtroom gameplay.
    ///
    /// An argument may target a statement and optionally a specific claim.
    /// Evidence-based arguments can contain multiple evidence items.
    ///
    /// Constructed at runtime by CourtroomController.
    /// Never authored or serialized.
    /// </summary>
    public sealed class PlayerArgumentData
    {
        private static readonly IReadOnlyList<EvidenceData> EmptyEvidence =
            Array.Empty<EvidenceData>();

        private static readonly IReadOnlyDictionary<string, string> EmptyContext =
            new Dictionary<string, string>();

        public PlayerArgumentData(
            PlayerAction action,
            IReadOnlyList<EvidenceData> evidence = null,
            StatementData selectedStatement = null,
            ClaimData selectedClaim = null,
            IReadOnlyDictionary<string, string> additionalContext = null)
        {
            Action = action;
            Evidence = evidence ?? EmptyEvidence;
            SelectedStatement = selectedStatement;
            SelectedClaim = selectedClaim;
            AdditionalContext = additionalContext ?? EmptyContext;
        }

        public PlayerAction Action { get; }

        /// <summary>
        /// Evidence presented as part of this argument.
        /// Multiple evidence items are allowed.
        /// </summary>
        public IReadOnlyList<EvidenceData> Evidence { get; }

        public StatementData SelectedStatement { get; }

        public ClaimData SelectedClaim { get; }

        public IReadOnlyDictionary<string, string> AdditionalContext { get; }

        public static PlayerArgumentData PresentEvidence(
            IEnumerable<EvidenceData> evidence,
            StatementData statement,
            ClaimData selectedClaim = null)
        {
            if (evidence == null)
                throw new ArgumentNullException(nameof(evidence));

            List<EvidenceData> evidenceList =
                evidence
                    .Where(e => e != null)
                    .Distinct()
                    .ToList();

            if (evidenceList.Count == 0)
            {
                throw new ArgumentException(
                    "At least one evidence item must be presented.",
                    nameof(evidence));
            }

            return new PlayerArgumentData(
                PlayerAction.PresentEvidence,
                evidenceList,
                statement,
                selectedClaim);
        }

        public static PlayerArgumentData Press(
            StatementData statement,
            ClaimData selectedClaim = null) =>
            new(
                PlayerAction.Press,
                selectedStatement: statement,
                selectedClaim: selectedClaim);

        public static PlayerArgumentData Question(
            StatementData statement,
            ClaimData selectedClaim = null) =>
            new(
                PlayerAction.Question,
                selectedStatement: statement,
                selectedClaim: selectedClaim);

        public static PlayerArgumentData RemainSilent(
            StatementData statement) =>
            new(
                PlayerAction.RemainSilent,
                selectedStatement: statement);
    }
}
