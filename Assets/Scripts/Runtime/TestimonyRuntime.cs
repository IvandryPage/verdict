using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Runtime.Witnesses
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

        public int CurrentStatementIndex { get; set; }

        public bool IsCompleted { get; set; }
    }
}
