using System;
using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    /// <summary>
    /// Pure gameplay runtime state for a statement. Does not know about
    /// dialogue or narrative at all - the narrative graph references
    /// statements by id via StatementNodeData, never the other way around.
    /// </summary>
    public sealed class StatementRuntime
    {
        public StatementRuntime(
            StatementData data,
            IReadOnlyList<ClaimRuntime> claims)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Claims = claims ?? throw new ArgumentNullException(nameof(claims)); ;
        }

        public StatementData Data { get; }

        public IReadOnlyList<ClaimRuntime> Claims { get; }

        public bool IsVisible { get; set; }

        public bool HasBeenVisited { get; set; }

        // public bool IsResolved =>
        //     Claims.All(x => x.IsResolved);
    }
}
