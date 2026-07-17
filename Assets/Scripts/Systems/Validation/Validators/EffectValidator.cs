using System.Collections.Generic;
using Verdict.Data.Cases;

namespace Verdict.Systems.Validation
{
    public sealed class EffectValidator : ICaseValidator
    {
        public void Validate(
            CaseData caseData,
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
                                ValidateEffects(
                                    statement,
                                    rule.SuccessEffects,
                                    result);

                                ValidateEffects(
                                    statement,
                                    rule.FailureEffects,
                                    result);
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateEffects(
            StatementData statement,
            IEnumerable<CourtStateEffectData> effects,
            ValidationResult result)
        {
            foreach (CourtStateEffectData effect in effects)
            {
                ValidateEffect(
                    statement,
                    effect,
                    result);
            }
        }

        private static void ValidateEffect(
            StatementData statement,
            CourtStateEffectData effect,
            ValidationResult result)
        {
            switch (effect.Effect)
            {
                case CourtStateEffect.None:
                    ValidateNone(statement, effect, result);
                    break;

                case CourtStateEffect.RevealStatement:
                case CourtStateEffect.JumpStatement:
                    ValidateTarget(statement, effect,
                        EffectTargetType.Statement, result);
                    break;

                case CourtStateEffect.RevealTestimony:
                case CourtStateEffect.JumpTestimony:
                    ValidateTarget(statement, effect,
                        EffectTargetType.Testimony, result);
                    break;

                case CourtStateEffect.RevealWitness:
                case CourtStateEffect.JumpWitness:
                    ValidateTarget(statement, effect,
                        EffectTargetType.Witness, result);
                    break;

                case CourtStateEffect.UnlockEvidence:
                    ValidateTarget(statement, effect,
                        EffectTargetType.Evidence, result);
                    break;

                case CourtStateEffect.ModifyCourtStat:
                    ValidateCourtStat(statement, effect, result);
                    break;

                case CourtStateEffect.ModifyCharacterStat:
                    ValidateCharacterStat(statement, effect, result);
                    break;
            }
        }

        private static void ValidateNone(
            StatementData statement,
            CourtStateEffectData effect,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == EffectTargetType.None,
                ValidationScope.Effect,
                "Effect 'None' must use TargetType.None.",
                statement.Id);

            ValidationUtility.Ensure(
                result,
                !effect.HasTarget,
                ValidationScope.Effect,
                "Effect 'None' must not define TargetId.",
                statement.Id);
        }

        private static void ValidateTarget(
            StatementData statement,
            CourtStateEffectData effect,
            EffectTargetType expectedTarget,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == expectedTarget,
                ValidationScope.Effect,
                $"'{effect.Effect}' requires TargetType.{expectedTarget}.",
                statement.Id);

            ValidationUtility.Ensure(
                result,
                effect.HasTarget,
                ValidationScope.Effect,
                $"'{effect.Effect}' requires a TargetId.",
                statement.Id);
        }

        private static void ValidateCourtStat(
            StatementData statement,
            CourtStateEffectData effect,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == EffectTargetType.None,
                ValidationScope.Effect,
                "ModifyCourtStat must use TargetType.None.",
                statement.Id);

            ValidationUtility.Ensure(
                result,
                !effect.HasTarget,
                ValidationScope.Effect,
                "ModifyCourtStat must not define TargetId.",
                statement.Id);
        }

        private static void ValidateCharacterStat(
            StatementData statement,
            CourtStateEffectData effect,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == EffectTargetType.Character,
                ValidationScope.Effect,
                "ModifyCharacterStat must use TargetType.Character.",
                statement.Id);

            ValidationUtility.Ensure(
                result,
                effect.HasTarget,
                ValidationScope.Effect,
                "ModifyCharacterStat requires a TargetId.",
                statement.Id);
        }
    }
}
