using Verdict.Data.Cases;
using Verdict.Runtime;

namespace Verdict.Systems.Evaluation
{
    /// <summary>
    /// Everything a condition needs to evaluate itself against: the case's
    /// runtime state, the statement currently being argued, and the
    /// player's current argument submission. Read-only by convention -
    /// nothing in Systems.Evaluation mutates through this.
    /// </summary>
    public sealed class ResolverContext
    {
        public ResolverContext(
            CaseRuntime caseRuntime,
            StatementRuntime statement,
            PlayerArgumentData argument)
        {
            Case = caseRuntime;
            Statement = statement;
            Argument = argument;
        }

        public CaseRuntime Case { get; }

        public StatementRuntime Statement { get; }

        public PlayerArgumentData Argument { get; }
    }
}
