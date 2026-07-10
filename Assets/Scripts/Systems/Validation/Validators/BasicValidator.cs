using System.Linq;
using Verdict.Data.Cases;

namespace Verdict.Systems.Validation
{
    public sealed class BasicValidator : ICaseValidator
    {
        public void Validate(
            CaseData caseData,
            ValidationResult result)
        {
            if (caseData == null)
            {
                result.AddError(
                    ValidationScope.Case,
                    "CaseData is null.");

                return;
            }

            ValidateEvidence(caseData, result);
            ValidateWitnesses(caseData, result);
            ValidateTruth(caseData, result);
            ValidateEndings(caseData, result);
        }

        private static void ValidateEvidence(
            CaseData caseData,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                caseData.Evidence != null,
                ValidationScope.Evidence,
                $"Case '{caseData.name}' has no evidence list.",
                caseData);

            if (caseData.Evidence == null)
            {
                return;
            }

            ValidationUtility.Ensure(
                result,
                caseData.Evidence.Count > 0,
                ValidationScope.Evidence,
                $"Case '{caseData.name}' must contain at least one evidence entry.",
                caseData);

            ValidationUtility.Ensure(
                result,
                caseData.Evidence.All(e => e != null),
                ValidationScope.Evidence,
                $"Case '{caseData.name}' contains a null evidence entry.",
                caseData);
        }

        private static void ValidateWitnesses(
            CaseData caseData,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                caseData.Witnesses != null,
                ValidationScope.Witness,
                $"Case '{caseData.name}' has no witness list.",
                caseData);

            if (caseData.Witnesses == null)
            {
                return;
            }

            ValidationUtility.Ensure(
                result,
                caseData.Witnesses.Count > 0,
                ValidationScope.Witness,
                $"Case '{caseData.name}' must contain at least one witness.",
                caseData);

            ValidationUtility.Ensure(
                result,
                caseData.Witnesses.All(w => w != null),
                ValidationScope.Witness,
                $"Case '{caseData.name}' contains a null witness entry.",
                caseData);
        }

        private static void ValidateTruth(
            CaseData caseData,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                caseData.Truth != null,
                ValidationScope.Truth,
                $"Case '{caseData.name}' has no TruthData assigned.",
                caseData);

            if (caseData.Truth == null)
            {
                return;
            }

            ValidationUtility.Ensure(
                result,
                caseData.Truth.Facts != null,
                ValidationScope.Truth,
                $"Case '{caseData.name}' has no fact list.",
                caseData);
        }

        private static void ValidateEndings(
            CaseData caseData,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                caseData.Endings != null,
                ValidationScope.Ending,
                $"Case '{caseData.name}' has no ending list.",
                caseData);

            if (caseData.Endings == null)
            {
                return;
            }

            ValidationUtility.Ensure(
                result,
                caseData.Endings.Count > 0,
                ValidationScope.Ending,
                $"Case '{caseData.name}' must contain at least one ending.",
                caseData);

            ValidationUtility.Ensure(
                result,
                caseData.Endings.All(e => e != null),
                ValidationScope.Ending,
                $"Case '{caseData.name}' contains a null ending entry.",
                caseData);
        }
    }
}
