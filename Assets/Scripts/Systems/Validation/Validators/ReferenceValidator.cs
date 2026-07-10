using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Systems.Validation;

namespace Verdict.Systems.Validation
{
    public sealed class ReferenceValidator : ICaseValidator
    {
        public void Validate(
            CaseData caseData,
            ValidationResult result)
        {
            HashSet<string> witnessIds = caseData.Witnesses
                .Select(w => w.Id)
                .ToHashSet();

            HashSet<string> characterIds = caseData.Witnesses
                .Where(w => w.Character != null)
                .Select(w => w.Character.Id)
                .ToHashSet();

            HashSet<string> testimonyIds = caseData.Witnesses
                .SelectMany(w => w.Testimonies)
                .Select(t => t.Id)
                .ToHashSet();

            HashSet<string> statementIds = caseData.Witnesses
                .SelectMany(w => w.Testimonies)
                .SelectMany(t => t.Statements)
                .Select(s => s.Id)
                .ToHashSet();

            HashSet<string> evidenceIds = caseData.Evidence
                .Where(e => e?.Evidence != null)
                .Select(e => e.Evidence.Id)
                .ToHashSet();

            HashSet<string> factIds = caseData.Truth.Facts
                .Select(f => f.Id)
                .ToHashSet();

            ValidateStatementReferences(
                caseData,
                statementIds,
                result);

            ValidateClaimReferences(
                caseData,
                factIds,
                result);

            ValidateEvaluationRuleReferences(
                caseData,
                evidenceIds,
                result);

            ValidateEffectReferences(
                caseData,
                statementIds,
                testimonyIds,
                witnessIds,
                characterIds,
                evidenceIds,
                result);
        }

        private static void ValidateStatementReferences(
            CaseData caseData,
            HashSet<string> statementIds,
            ValidationResult result)
        {
            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        if (string.IsNullOrWhiteSpace(statement.NextStatementId))
                            continue;

                        ValidationUtility.Ensure(
                            result,
                            statementIds.Contains(statement.NextStatementId),
                            ValidationScope.Statement,
                            $"Statement '{statement.Id}' references unknown NextStatement '{statement.NextStatementId}'.",
                            caseData);
                    }
                }
            }
        }

        private static void ValidateClaimReferences(
            CaseData caseData,
            HashSet<string> factIds,
            ValidationResult result)
        {
            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        foreach (ClaimData claim in statement.Claims)
                        {
                            ValidationUtility.Ensure(
                                result,
                                !string.IsNullOrWhiteSpace(claim.FactId),
                                ValidationScope.Claim,
                                $"Claim '{claim.Id}' has no FactId.",
                                caseData);

                            if (string.IsNullOrWhiteSpace(claim.FactId))
                                continue;

                            ValidationUtility.Ensure(
                                result,
                                factIds.Contains(claim.FactId),
                                ValidationScope.Claim,
                                $"Claim '{claim.Id}' references unknown Fact '{claim.FactId}'.",
                                caseData);
                        }
                    }
                }
            }
        }

        private static void ValidateEvaluationRuleReferences(
            CaseData caseData,
            HashSet<string> evidenceIds,
            ValidationResult result)
        {
            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        foreach (ClaimData claim in statement.Claims)
                        {
                            foreach (EvaluationRuleData rule in claim.EvaluationRules)
                            {
                                if (rule.EvaluationType != EvaluationType.PresentEvidence)
                                    continue;

                                ValidationUtility.Ensure(
                                    result,
                                    rule.RequiredEvidence != null,
                                    ValidationScope.Rule,
                                    $"PresentEvidence rule in Claim '{claim.Id}' has no RequiredEvidence.",
                                    caseData);

                                if (rule.RequiredEvidence == null)
                                    continue;

                                ValidationUtility.Ensure(
                                    result,
                                    evidenceIds.Contains(rule.RequiredEvidence.Id),
                                    ValidationScope.Rule,
                                    $"PresentEvidence rule in Claim '{claim.Id}' references Evidence '{rule.RequiredEvidence.Id}' which is not part of this case.",
                                    caseData);
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateEffectReferences(
            CaseData caseData,
            HashSet<string> statementIds,
            HashSet<string> testimonyIds,
            HashSet<string> witnessIds,
            HashSet<string> characterIds,
            HashSet<string> evidenceIds,
            ValidationResult result)
        {
            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        foreach (ClaimData claim in statement.Claims)
                        {
                            ValidateEffects(
                                claim,
                                claim.EvaluationRules.SelectMany(r => r.SuccessEffects),
                                statementIds,
                                testimonyIds,
                                witnessIds,
                                characterIds,
                                evidenceIds,
                                result,
                                caseData);

                            ValidateEffects(
                                claim,
                                claim.EvaluationRules.SelectMany(r => r.FailureEffects),
                                statementIds,
                                testimonyIds,
                                witnessIds,
                                characterIds,
                                evidenceIds,
                                result,
                                caseData);
                        }
                    }
                }
            }
        }

        private static void ValidateEffects(
            ClaimData claim,
            IEnumerable<CourtStateEffectData> effects,
            HashSet<string> statementIds,
            HashSet<string> testimonyIds,
            HashSet<string> witnessIds,
            HashSet<string> characterIds,
            HashSet<string> evidenceIds,
            ValidationResult result,
            CaseData source)
        {
            foreach (CourtStateEffectData effect in effects)
            {
                if (effect == null)
                    continue;

                switch (effect.TargetType)
                {
                    case EffectTargetType.None:
                        break;

                    case EffectTargetType.Statement:
                        ValidationUtility.Ensure(
                            result,
                            statementIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Statement '{effect.TargetId}'.",
                            source);
                        break;

                    case EffectTargetType.Testimony:
                        ValidationUtility.Ensure(
                            result,
                            testimonyIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Testimony '{effect.TargetId}'.",
                            source);
                        break;

                    case EffectTargetType.Witness:
                        ValidationUtility.Ensure(
                            result,
                            witnessIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Witness '{effect.TargetId}'.",
                            source);
                        break;

                    case EffectTargetType.Character:
                        ValidationUtility.Ensure(
                            result,
                            characterIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Character '{effect.TargetId}'.",
                            source);
                        break;

                    case EffectTargetType.Evidence:
                        ValidationUtility.Ensure(
                            result,
                            evidenceIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Evidence '{effect.TargetId}'.",
                            source);
                        break;
                }
            }
        }
    }
}
