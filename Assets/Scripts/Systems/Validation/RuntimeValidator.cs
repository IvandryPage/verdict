using Verdict.Data.Cases;

namespace Verdict.Systems.Validation
{
    public static class RuntimeValidator
    {
        private static readonly ICaseValidator[] Validators =
        {
            new BasicValidator(),
            new IdValidator(),
            new ReferenceValidator(),
            // new EffectValidator(),
            // new FlowValidator(),
        };

        public static ValidationResult Validate(
            CaseData caseData)
        {
            ValidationResult result = new();

            foreach (ICaseValidator validator in Validators)
            {
                validator.Validate(caseData, result);
            }

            return result;
        }
    }
}
