using System.Collections.Generic;
using UnityEngine;
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

            for (int w = 0; w < caseData.Witnesses.Count; w++)
            {
                WitnessData witness = caseData.Witnesses[w];

                for (int t = 0; t < witness.Testimonies.Count; t++)
                {
                    TestimonyData testimony = witness.Testimonies[t];

                    for (int s = 0; s < testimony.Statements.Count; s++)
                    {
                        StatementData statement = testimony.Statements[s];

                        statementContexts.Add(
                            statement.Id,
                            new StatementContext(
                                caseData,
                                witness,
                                testimony,
                                statement,
                                w,
                                t,
                                s));
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

        public StatementContext GetContext(
            string statementId)
        {
            return statementContexts[statementId];
        }
    }
}
