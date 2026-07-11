using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseFlow
{
    public sealed class EditorSession
    {
        private readonly Dictionary<string, StatementContext> statementContexts =
            new();

        public CaseData CurrentCase { get; private set; }

        public FlowGraph FlowGraph { get; private set; }

        public EditorSelection Selection { get; }

        public bool HasCase => CurrentCase != null;

        public EditorSession()
        {
            Selection = new EditorSelection();
        }

        public void LoadCase(
            CaseData caseData)
        {
            CurrentCase = caseData;

            Selection.Clear();

            statementContexts.Clear();

            FlowGraph =
                FlowGraphBuilder.Build(caseData);

            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        statementContexts.Add(
                            statement.Id,
                            new StatementContext(
                                statement,
                                testimony,
                                witness));
                    }
                }
            }
        }

        public bool TryGetContext(
            string statementId,
            out StatementContext context)
        {
            return statementContexts.TryGetValue(
                statementId,
                out context);
        }

        public void Clear()
        {
            CurrentCase = null;
            FlowGraph = null;

            Selection.Clear();

            statementContexts.Clear();
        }
    }
}
