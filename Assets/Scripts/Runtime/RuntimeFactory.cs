using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public static class RuntimeFactory
    {
        public static CaseRuntime Create(CaseData data)
        {
            IReadOnlyList<EvidenceRuntime> evidence = CreateEvidence(data);

            IReadOnlyList<WitnessRuntime> witnesses = CreateWitnesses(data);

            CourtStateRuntime courtState = new();

            return new CaseRuntime(
                data,
                evidence,
                witnesses,
                courtState);
        }

        private static IReadOnlyList<EvidenceRuntime> CreateEvidence(CaseData data)
        {
            return data.Evidence
                .Select(e => new EvidenceRuntime(e))
                .ToList();
        }

        private static IReadOnlyList<WitnessRuntime> CreateWitnesses(CaseData data)
        {
            return data.Witnesses
                .Select(CreateWitness)
                .ToList();
        }

        private static WitnessRuntime CreateWitness(WitnessData witness)
        {
            IReadOnlyList<TestimonyRuntime> testimonies =
                witness.Testimonies
                    .Select(CreateTestimony)
                    .ToList();

            return new WitnessRuntime(
                witness,
                testimonies);
        }

        private static TestimonyRuntime CreateTestimony(TestimonyData testimony)
        {
            IReadOnlyList<StatementRuntime> statements =
                testimony.Statements
                    .Select(CreateStatement)
                    .ToList();

            return new TestimonyRuntime(
                testimony,
                statements);
        }

        private static StatementRuntime CreateStatement(StatementData statement)
        {
            return new StatementRuntime(statement);
        }
    }
}
