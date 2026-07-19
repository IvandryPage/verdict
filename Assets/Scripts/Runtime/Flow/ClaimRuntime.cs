using System;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    /// <summary>
    /// Runtime state for a single claim during gameplay.
    /// A claim can only be resolved once.
    /// </summary>
    public sealed class ClaimRuntime
    {
        public ClaimRuntime(ClaimData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Immutable authoring data.
        /// </summary>
        public ClaimData Data { get; }

        /// <summary>
        /// Player has interacted with this claim at least once.
        /// </summary>
        public bool HasBeenAttempted { get; set; }

        /// <summary>
        /// Claim already reached its final outcome.
        /// Once true, Resolver ignores this claim forever.
        /// </summary>
        public bool IsResolved { get; set; }

        /// <summary>
        /// Final result of the resolution.
        /// </summary>
        public bool WasSuccessful { get; set; }

        /// <summary>
        /// Total player attempts.
        /// Mainly for analytics, achievements,
        /// or adaptive narrative.
        /// </summary>
        public int AttemptCount { get; set; }

        /// <summary>
        /// Shortcut.
        /// </summary>
        public bool CanResolve => !IsResolved;
    }
}
