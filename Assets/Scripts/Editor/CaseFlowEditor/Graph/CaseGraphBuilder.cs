using Verdict.Data.Cases;

namespace Verdict.Editor.CaseFlow
{
    public sealed class CaseGraphBuilder
    {
        private readonly CaseGraphView graphView;

        public CaseGraphBuilder(
            CaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void Build(CaseData caseData)
        {
            graphView.ClearGraph();

            float x = 100;
            float y = 100;

            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        graphView.CreateStatementNode(
                            statement,
                            witness,
                            testimony,
                            new UnityEngine.Vector2(x, y));

                        x += 350;

                        if (x > 1800)
                        {
                            x = 100;
                            y += 250;
                        }
                    }
                }
            }
        }
    }
}
