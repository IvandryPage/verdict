using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class EditorSession
    {
        public CaseData CurrentCase { get; private set; }

        public EditorSelection Selection { get; }

        private readonly Dictionary<StatementData, WitnessData> statementToWitness =
            new();

        private readonly Dictionary<StatementData, TestimonyData> statementToTestimony =
            new();

        public EditorSession()
        {
            Selection = new EditorSelection();
        }

        public void LoadCase(CaseData caseData)
        {
            CurrentCase = caseData;

            statementToWitness.Clear();
            statementToTestimony.Clear();

            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        statementToWitness.Add(statement, witness);
                        statementToTestimony.Add(statement, testimony);
                    }
                }
            }
        }

        public WitnessData GetWitness(
            StatementData statement)
        {
            return statementToWitness.GetValueOrDefault(statement);
        }

        public TestimonyData GetTestimony(
            StatementData statement)
        {
            return statementToTestimony.GetValueOrDefault(statement);
        }

        public void Clear()
        {
            CurrentCase = null;
        }
    }
}
