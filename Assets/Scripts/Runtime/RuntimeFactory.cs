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
                data.Evidence.All(e => e != null),
                $"Case '{data.name}' contains a null evidence entry.");

            Ensure(
                data.Witnesses != null,
                $"Case '{data.name}' has no witness list.");

            Ensure(
                data.Witnesses.All(w => w != null),
                $"Case '{data.name}' contains a null witness entry.");

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
                witness.Character != null,
                $"Witness '{witness.Id}' has no character assigned.");

            Ensure(
                witness.Testimonies != null,
                $"Witness '{witness.Character.name}' has no testimony list.");

            Ensure(
                witness.Testimonies.All(t => t != null),
                $"Witness '{witness.Character.name}' contains a null testimony entry.");

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
                testimony.Statements.All(s => s != null),
                $"Testimony '{testimony.Title}' contains a null statement entry.");

            Ensure(
                testimony.Statements.Count > 0,
                $"Testimony '{testimony.Title}' must contain at least one statement.");
        }

        private static void ValidateStatement(StatementData statement)
        {
            Ensure(
                statement != null,
                "Encountered a null StatementData.");

            Ensure(
                statement.Claims != null,
                $"Statement '{statement.Id}' has no claims list.");

            Ensure(
                statement.Claims.All(c => c != null),
                $"Statement '{statement.Id}' contains a null claim entry.");

            foreach (ClaimData claim in statement.Claims)
            {
                ValidateClaim(claim);
            }
        }

        private static void ValidateClaim(ClaimData claim)
        {
            Ensure(
                claim != null,
                "Encountered a null ClaimData.");

            Ensure(
                claim.EvaluationRules != null,
                $"Claim '{claim.Id}' has no evaluation rules list.");

            Ensure(
                claim.EvaluationRules.All(r => r != null),
                $"Claim '{claim.Id}' contains a null evaluation rule entry.");

            foreach (EvaluationRuleData rule in claim.EvaluationRules)
            {
                ValidateEvaluationRule(rule);
            }
        }

        private static void ValidateEvaluationRule(EvaluationRuleData rule)
        {
            Ensure(
                rule != null,
                "Encountered a null EvaluationRuleData.");

            Ensure(
                rule.SuccessEffects != null,
                "Encountered a null SuccessEffects list.");

            Ensure(
                rule.SuccessEffects.All(e => e != null),
                "Encountered a null CourtStateEffectData in SuccessEffects.");

            Ensure(
                rule.FailureEffects != null,
                "Encountered a null FailureEffects list.");

            Ensure(
                rule.FailureEffects.All(e => e != null),
                "Encountered a null CourtStateEffectData in FailureEffects.");
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
