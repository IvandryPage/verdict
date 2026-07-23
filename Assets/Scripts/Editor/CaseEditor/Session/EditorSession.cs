using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Systems.Validation.Graph;

namespace Verdict.Editor.CaseEditor
{
    public sealed class EditorSession
    {
        private readonly Dictionary<string, StatementContext> statementContexts =
            new();

        private readonly Dictionary<string, WitnessContext> witnessContexts =
            new();

        private readonly Dictionary<string, TestimonyContext> testimonyContexts =
            new();

        private readonly Dictionary<string, string> claimToStatementId =
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

            witnessContexts.Clear();

            testimonyContexts.Clear();

            claimToStatementId.Clear();


            for (int w = 0; w < caseData.Witnesses.Count; w++)
            {
                WitnessData witness = caseData.Witnesses[w];


                witnessContexts.Add(
                    witness.Id,
                    new WitnessContext(
                        caseData,
                        witness,
                        w));


                for (int t = 0; t < witness.Testimonies.Count; t++)
                {
                    TestimonyData testimony =
                        witness.Testimonies[t];


                    testimonyContexts.Add(
                        testimony.Id,
                        new TestimonyContext(
                            caseData,
                            witness,
                            testimony,
                            w,
                            t));


                    for (int s = 0; s < testimony.Statements.Count; s++)
                    {
                        StatementData statement =
                            testimony.Statements[s];


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

                        foreach (ClaimData claim in statement.Claims)
                        {
                            if (!string.IsNullOrWhiteSpace(claim.Id))
                            {
                                claimToStatementId[claim.Id] = statement.Id;
                            }
                        }
                    }
                }
            }


            FlowGraph =
                FlowGraphBuilder.Build(caseData);
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

        public StatementContext CreateStatementContext(
            WitnessData witness,
            TestimonyData testimony,
            StatementData statement)
        {
            int wIndex =
                FindIndex(
                    CurrentCase.Witnesses,
                    witness);

            int tIndex =
                FindIndex(
                    witness.Testimonies,
                    testimony);

            int sIndex =
                FindIndex(
                    testimony.Statements,
                    statement);


            if (wIndex < 0 ||
                tIndex < 0 ||
                sIndex < 0)
            {
                return null;
            }


            return new StatementContext(
                CurrentCase,
                witness,
                testimony,
                statement,
                wIndex,
                tIndex,
                sIndex);
        }

        public TestimonyContext CreateTestimonyContext(
            WitnessData witness,
            TestimonyData testimony)
        {
            int wIndex =
                FindIndex(
                    CurrentCase.Witnesses,
                    witness);


            int tIndex =
                FindIndex(
                    witness.Testimonies,
                    testimony);


            if (wIndex < 0 || tIndex < 0)
                return null;


            return new TestimonyContext(
                CurrentCase,
                witness,
                testimony,
                wIndex,
                tIndex);
        }

        private int FindIndex<T>(
            IReadOnlyList<T> list,
            T target)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(
                    list[i],
                    target))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool TryGetWitnessContext(
            string id,
            out WitnessContext context)
        {
            return witnessContexts.TryGetValue(
                id,
                out context);
        }


        public bool TryGetTestimonyContext(
            string id,
            out TestimonyContext context)
        {
            return testimonyContexts.TryGetValue(
                id,
                out context);
        }

        /// <summary>
        /// Finds the StatementContext owning a Claim - used to "focus"
        /// validation issues whose ContextId is a Claim/Rule/Effect id
        /// rather than a Statement id directly.
        /// </summary>
        public bool TryGetContextForClaim(
            string claimId,
            out StatementContext context)
        {
            if (!string.IsNullOrWhiteSpace(claimId) &&
                claimToStatementId.TryGetValue(claimId, out string statementId))
            {
                return TryGetContext(statementId, out context);
            }

            context = null;
            return false;
        }
    }
}
