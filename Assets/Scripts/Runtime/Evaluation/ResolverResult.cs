using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    /// <summary>
    /// One claim's outcome from a single resolve pass - which claim,
    /// which rule matched its action, and whether every condition on
    /// that rule passed.
    /// </summary>
    public sealed class ResolvedClaim
    {
        public ResolvedClaim(
            ClaimRuntime claim,
            ArgumentRuleData rule,
            bool isSuccess)
        {
            Claim = claim;
            Rule = rule;
            IsSuccess = isSuccess;
        }

        public ClaimRuntime Claim { get; }

        public ArgumentRuleData Rule { get; }

        public bool IsSuccess { get; }
    }

    /// <summary>
    /// Pure result of resolving a PlayerArgumentData against a statement's
    /// claims. A single argument can resolve several claims at once (e.g.
    /// one piece of evidence disproving two claims simultaneously) - this
    /// is never just one match. ResolverEngine only ever returns this, it
    /// never mutates gameplay state itself. GeneratedEffects is already
    /// flattened and ready for CourtStateEffectProcessor to apply as-is.
    /// </summary>
    public sealed class ResolverResult
    {
        public ResolverResult(
            bool isSuccess,
            IReadOnlyList<ResolvedClaim> resolvedClaims,
            IReadOnlyList<CourtStateEffectData> generatedEffects,
            IReadOnlyList<string> diagnostics)
        {
            IsSuccess = isSuccess;
            ResolvedClaims = resolvedClaims;
            GeneratedEffects = generatedEffects;
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// True if at least one claim resolved successfully this pass.
        /// </summary>
        public bool IsSuccess { get; }

        public IReadOnlyList<ResolvedClaim> ResolvedClaims { get; }

        public IReadOnlyList<CourtStateEffectData> GeneratedEffects { get; }

        public IReadOnlyList<string> Diagnostics { get; }

        public bool HasResolvedClaims => ResolvedClaims.Count > 0;
    }
}
