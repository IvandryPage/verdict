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

            ValidateArgumentRuleReferences(
                caseData,
                evidenceIds,
                factIds,
                result);

            ValidateNoOrphanFacts(
                caseData,
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
                        ValidationUtility.Ensure(
                            result,
                            statement.Claims != null && statement.Claims.Count > 0,
                            ValidationScope.Statement,
                            $"Statement '{statement.Id}' has no claims.",
                            testimony.Id);

                        if (string.IsNullOrWhiteSpace(statement.NextStatementId))
                            continue;

                        ValidationUtility.Ensure(
                            result,
                            statementIds.Contains(statement.NextStatementId),
                            ValidationScope.Statement,
                            $"Statement '{statement.Id}' references unknown NextStatement '{statement.NextStatementId}'.",
                            testimony.Id);
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
                                statement.Id);

                            if (string.IsNullOrWhiteSpace(claim.FactId))
                                continue;

                            ValidationUtility.Ensure(
                                result,
                                factIds.Contains(claim.FactId),
                                ValidationScope.Claim,
                                $"Claim '{claim.Id}' references unknown Fact '{claim.FactId}'.",
                                statement.Id);
                        }
                    }
                }
            }
        }

        private static void ValidateNoOrphanFacts(
            CaseData caseData,
            ValidationResult result)
        {
            HashSet<string> referencedFactIds = new();

            foreach (WitnessData witness in caseData.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        foreach (ClaimData claim in statement.Claims)
                        {
                            if (!string.IsNullOrWhiteSpace(claim.FactId))
                            {
                                referencedFactIds.Add(claim.FactId);
                            }

                            foreach (ArgumentRuleData rule in claim.ArgumentRules)
                            {
                                foreach (ArgumentConditionData condition in rule.Conditions)
                                {
                                    if (condition is FactConditionData factCondition &&
                                        !string.IsNullOrWhiteSpace(factCondition.FactId))
                                    {
                                        referencedFactIds.Add(factCondition.FactId);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (FactData fact in caseData.Truth.Facts)
            {
                ValidationUtility.Warning(
                    result,
                    referencedFactIds.Contains(fact.Id),
                    ValidationScope.Fact,
                    $"Fact '{fact.Id}' is not referenced by any Claim or FactCondition - it can never be proven or matter to gameplay.",
                    fact.Id);
            }
        }

        private static void ValidateArgumentRuleReferences(
            CaseData caseData,
            HashSet<string> evidenceIds,
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
                            foreach (ArgumentRuleData rule in claim.ArgumentRules)
                            {
                                ValidationUtility.Ensure(
                                    result,
                                    rule.Conditions != null && rule.Conditions.Count > 0,
                                    ValidationScope.Rule,
                                    $"Argument rule on Claim '{claim.Id}' has no conditions.",
                                    claim.Id);

                                foreach (ArgumentConditionData condition in rule.Conditions)
                                {
                                    ValidateCondition(
                                        claim,
                                        condition,
                                        evidenceIds,
                                        factIds,
                                        result);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateCondition(
            ClaimData claim,
            ArgumentConditionData condition,
            HashSet<string> evidenceIds,
            HashSet<string> factIds,
            ValidationResult result)
        {
            switch (condition)
            {
                case EvidenceConditionData evidenceCondition:

                    ValidationUtility.Ensure(
                        result,
                        evidenceCondition.RequiredEvidence != null,
                        ValidationScope.Rule,
                        $"Evidence condition in Claim '{claim.Id}' has no RequiredEvidence.",
                        claim.Id);

                    if (evidenceCondition.RequiredEvidence != null)
                    {
                        ValidationUtility.Ensure(
                            result,
                            evidenceIds.Contains(evidenceCondition.RequiredEvidence.Id),
                            ValidationScope.Rule,
                            $"Evidence condition in Claim '{claim.Id}' references Evidence '{evidenceCondition.RequiredEvidence.Id}' which is not part of this case.",
                            claim.Id);
                    }

                    break;

                case FactConditionData factCondition:

                    ValidationUtility.Ensure(
                        result,
                        !string.IsNullOrWhiteSpace(factCondition.FactId),
                        ValidationScope.Rule,
                        $"Fact condition in Claim '{claim.Id}' has no FactId.",
                        claim.Id);

                    if (!string.IsNullOrWhiteSpace(factCondition.FactId))
                    {
                        ValidationUtility.Ensure(
                            result,
                            factIds.Contains(factCondition.FactId),
                            ValidationScope.Rule,
                            $"Fact condition in Claim '{claim.Id}' references unknown Fact '{factCondition.FactId}'.",
                            claim.Id);
                    }

                    break;

                case ClaimConditionData claimCondition:

                    ValidationUtility.Ensure(
                        result,
                        !string.IsNullOrWhiteSpace(claimCondition.ClaimId),
                        ValidationScope.Rule,
                        $"Claim condition in Claim '{claim.Id}' has no target ClaimId.",
                        claim.Id);

                    break;
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
                                claim.ArgumentRules.SelectMany(r => r.SuccessEffects),
                                statementIds,
                                testimonyIds,
                                witnessIds,
                                characterIds,
                                evidenceIds,
                                result,
                                statement.Id);

                            ValidateEffects(
                                claim,
                                claim.ArgumentRules.SelectMany(r => r.FailureEffects),
                                statementIds,
                                testimonyIds,
                                witnessIds,
                                characterIds,
                                evidenceIds,
                                result,
                                statement.Id);
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
            string contextId)
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
                            contextId);
                        break;

                    case EffectTargetType.Testimony:
                        ValidationUtility.Ensure(
                            result,
                            testimonyIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Testimony '{effect.TargetId}'.",
                            contextId);
                        break;

                    case EffectTargetType.Witness:
                        ValidationUtility.Ensure(
                            result,
                            witnessIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Witness '{effect.TargetId}'.",
                            contextId);
                        break;

                    case EffectTargetType.Character:
                        ValidationUtility.Ensure(
                            result,
                            characterIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Character '{effect.TargetId}'.",
                            contextId);
                        break;

                    case EffectTargetType.Evidence:
                        ValidationUtility.Ensure(
                            result,
                            evidenceIds.Contains(effect.TargetId),
                            ValidationScope.Effect,
                            $"Effect '{effect.Effect}' references unknown Evidence '{effect.TargetId}'.",
                            claim.Id);
                        break;
                }
            }
        }
    }
}
