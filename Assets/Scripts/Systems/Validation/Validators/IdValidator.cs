using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Systems.Validation
{
    public sealed class IdValidator : ICaseValidator
    {
        public void Validate(
            CaseData caseData,
            ValidationResult result)
        {
            ValidateEvidenceIds(caseData, result);
            ValidateFactIds(caseData, result);
            ValidateWitnessIds(caseData, result);
            ValidateEndingIds(caseData, result);
        }

        private static void ValidateWitnessIds(
            CaseData caseData,
            ValidationResult result)
        {
            var witnessIds = new HashSet<string>();
            var testimonyIds = new HashSet<string>();
            var statementIds = new HashSet<string>();
            var claimIds = new HashSet<string>();

            foreach (WitnessData witness in caseData.Witnesses)
            {
                ValidateId(
                    witnessIds,
                    witness.Id,
                    "Witness",
                    ValidationScope.Witness,
                    caseData,
                    result);

                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    ValidateId(
                        testimonyIds,
                        testimony.Id,
                        "Testimony",
                        ValidationScope.Testimony,
                        caseData,
                        result);

                    foreach (StatementData statement in testimony.Statements)
                    {
                        ValidateId(
                            statementIds,
                            statement.Id,
                            "Statement",
                            ValidationScope.Statement,
                            caseData,
                            result);

                        foreach (ClaimData claim in statement.Claims)
                        {
                            ValidateId(
                                claimIds,
                                claim.Id,
                                "Claim",
                                ValidationScope.Claim,
                                caseData,
                                result);
                        }
                    }
                }
            }
        }

        private static void ValidateEvidenceIds(
            CaseData caseData,
            ValidationResult result)
        {
            var ids = new HashSet<string>();

            foreach (EvidenceEntryData entry in caseData.Evidence)
            {
                if (entry?.Evidence == null)
                    continue;

                ValidateId(
                    ids,
                    entry.Evidence.Id,
                    "Evidence",
                    ValidationScope.Evidence,
                    entry.Evidence,
                    result);
            }
        }

        private static void ValidateFactIds(
            CaseData caseData,
            ValidationResult result)
        {
            var ids = new HashSet<string>();

            foreach (FactData fact in caseData.Truth.Facts)
            {
                ValidateId(
                    ids,
                    fact.Id,
                    "Fact",
                    ValidationScope.Truth,
                    caseData,
                    result);
            }
        }

        private static void ValidateEndingIds(
            CaseData caseData,
            ValidationResult result)
        {
            var ids = new HashSet<string>();

            foreach (EndingData ending in caseData.Endings)
            {
                ValidateId(
                    ids,
                    ending.Id,
                    "Ending",
                    ValidationScope.Ending,
                    caseData,
                    result);
            }
        }

        private static void ValidateId(
            HashSet<string> ids,
            string id,
            string displayName,
            ValidationScope scope,
            UnityEngine.Object source,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                !string.IsNullOrWhiteSpace(id),
                scope,
                $"{displayName} has no ID.",
                source);

            if (string.IsNullOrWhiteSpace(id))
                return;

            ValidationUtility.Ensure(
                result,
                ids.Add(id),
                scope,
                $"Duplicate {displayName} ID '{id}'.",
                source);
        }
    }
}
