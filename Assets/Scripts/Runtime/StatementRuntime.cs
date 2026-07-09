using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class StatementRuntime
    {
        public StatementRuntime(StatementData data)
        {
            Data = data;
        }

        public StatementData Data { get; }

        public bool HasBeenVisited { get; set; }

        public bool HasBeenPressed { get; set; }

        public bool HasBeenResolved { get; set; }
    }
}
