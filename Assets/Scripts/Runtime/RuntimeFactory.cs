using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;

namespace Verdict.Runtime
{
    public static class RuntimeFactory
    {
        public static CaseRuntime Create(CaseData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            ValidateCase(data);

            IReadOnlyList<EvidenceRuntime> evidence =
                CreateEvidence(data);

            IReadOnlyList<WitnessRuntime> witnesses =
                CreateWitnesses(data);

            CourtStateRuntime courtState = new();

            return new CaseRuntime(
                data,
                evidence,
                witnesses,
                courtState);
        }

        private static IReadOnlyList<EvidenceRuntime> CreateEvidence(
            CaseData data)
        {
            return data.Evidence
                .Select(e => new EvidenceRuntime(e))
                .ToList();
        }

        private static IReadOnlyList<WitnessRuntime> CreateWitnesses(
            CaseData data)
        {
            return data.Witnesses
                .Select(CreateWitness)
                .ToList();
        }

        private static WitnessRuntime CreateWitness(
            WitnessData witness)
        {
            ValidateWitness(witness);

            IReadOnlyList<TestimonyRuntime> testimonies =
                witness.Testimonies
                    .Select(CreateTestimony)
                    .ToList();

            return new WitnessRuntime(
                witness,
                testimonies);
        }

        private static TestimonyRuntime CreateTestimony(
            TestimonyData testimony)
        {
            ValidateTestimony(testimony);

            IReadOnlyList<StatementRuntime> statements =
                testimony.Statements
                    .Select(CreateStatement)
                    .ToList();

            return new TestimonyRuntime(
                testimony,
                statements);
        }

        private static StatementRuntime CreateStatement(
            StatementData statement)
        {
            ValidateStatement(statement);

            return new StatementRuntime(statement);
        }

        // ------------------------------------------------------------------
        // Validation
        // ------------------------------------------------------------------

        private static void ValidateCase(CaseData data)
        {
            Ensure(
                data.Evidence != null,
                $"Case '{data.name}' has no evidence list.");

            Ensure(
                data.Witnesses != null,
                $"Case '{data.name}' has no witness list.");

            Ensure(
                data.Witnesses.Count > 0,
                $"Case '{data.name}' must contain at least one witness.");
        }

        private static void ValidateWitness(WitnessData witness)
        {
            Ensure(
                witness != null,
                "Encountered a null WitnessData.");

            Ensure(
                witness.Testimonies != null,
                $"Witness '{witness.Character.name}' has no testimony list.");

            Ensure(
                witness.Testimonies.Count > 0,
                $"Witness '{witness.Character.name}' must contain at least one testimony.");
        }

        private static void ValidateTestimony(TestimonyData testimony)
        {
            Ensure(
                testimony != null,
                "Encountered a null TestimonyData.");

            Ensure(
                testimony.Statements != null,
                $"Testimony '{testimony.Title}' has no statement list.");

            Ensure(
                testimony.Statements.Count > 0,
                $"Testimony '{testimony.Title}' must contain at least one statement.");
        }

        private static void ValidateStatement(StatementData statement)
        {
            Ensure(
                statement != null,
                "Encountered a null StatementData.");
        }

        private static void Ensure(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
