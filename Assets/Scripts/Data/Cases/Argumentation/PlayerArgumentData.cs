using System.Collections.Generic;
using Verdict.Data.Evidence;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// A single argument the player just made: an action, optionally
    /// paired with evidence, targeting a statement (and optionally a
    /// specific claim on it), plus a free-form context bag for
    /// authoring-driven conditions that don't need new C# code.
    /// Constructed at runtime by CourtroomController from a player input -
    /// never authored, never serialized.
    /// </summary>
    public sealed class PlayerArgumentData
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyContext =
            new Dictionary<string, string>();

        public PlayerArgumentData(
            PlayerAction action,
            EvidenceData evidence = null,
            StatementData selectedStatement = null,
            ClaimData selectedClaim = null,
            IReadOnlyDictionary<string, string> additionalContext = null)
        {
            Action = action;
            Evidence = evidence;
            SelectedStatement = selectedStatement;
            SelectedClaim = selectedClaim;
            AdditionalContext = additionalContext ?? EmptyContext;
        }

        public PlayerAction Action { get; }

        public EvidenceData Evidence { get; }

        /// <summary>
        /// The statement this argument is being made against. Resolver
        /// uses this to find which claims to evaluate.
        /// </summary>
        public StatementData SelectedStatement { get; }

        /// <summary>
        /// If set, narrows resolution to this one claim instead of every
        /// unresolved claim on the statement - used when the player (or
        /// UI) explicitly targets a specific claim.
        /// </summary>
        public ClaimData SelectedClaim { get; }

        /// <summary>
        /// Free-form key/value context for future condition types
        /// (dialogue choices, minigame results, etc) without requiring
        /// new PlayerArgumentData fields per feature.
        /// </summary>
        public IReadOnlyDictionary<string, string> AdditionalContext { get; }

        public static PlayerArgumentData PresentEvidence(
            EvidenceData evidence,
            StatementData statement,
            ClaimData selectedClaim = null) =>
            new(PlayerAction.PresentEvidence, evidence, statement, selectedClaim);

        public static PlayerArgumentData Press(
            StatementData statement,
            ClaimData selectedClaim = null) =>
            new(PlayerAction.Press, selectedStatement: statement, selectedClaim: selectedClaim);

        public static PlayerArgumentData Question(
            StatementData statement,
            ClaimData selectedClaim = null) =>
            new(PlayerAction.Question, selectedStatement: statement, selectedClaim: selectedClaim);

        public static PlayerArgumentData RemainSilent(
            StatementData statement) =>
            new(PlayerAction.RemainSilent, selectedStatement: statement);
    }
}
