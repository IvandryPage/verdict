using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public sealed class TestimonyRuntime
    {
        public TestimonyRuntime(
            TestimonyData data,
            IReadOnlyList<StatementRuntime> statements)
        {
            Data = data;
            Statements = statements;
        }

        public TestimonyData Data { get; }

        public IReadOnlyList<StatementRuntime> Statements { get; }

        public bool IsCompleted { get; set; }
    }
}
