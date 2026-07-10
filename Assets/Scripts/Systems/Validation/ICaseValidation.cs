using Verdict.Data.Cases;

namespace Verdict.Systems.Validation
{
    public interface ICaseValidator
    {
        void Validate(
            CaseData caseData,
            ValidationResult result);
    }
}
