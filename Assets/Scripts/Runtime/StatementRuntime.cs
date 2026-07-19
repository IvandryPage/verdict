using System;
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
            StatementData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public StatementData Data { get; }

        public bool IsVisible { get; set; }

        public bool HasBeenVisited { get; set; }

        public bool HasBeenPressed { get; set; }

        public bool HasBeenResolved { get; set; }
    }
}
