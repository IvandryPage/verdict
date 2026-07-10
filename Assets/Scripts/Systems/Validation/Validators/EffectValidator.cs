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
                                    rule.SuccessEffects,
                                    result);

                                ValidateEffects(
                                    rule.FailureEffects,
                                    result);
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateEffects(
            System.Collections.Generic.IEnumerable<CourtStateEffectData> effects,
            ValidationResult result)
        {
            foreach (CourtStateEffectData effect in effects)
            {
                ValidateEffect(effect, result);
            }
        }

        private static void ValidateEffect(
            CourtStateEffectData effect,
            ValidationResult result)
        {
            switch (effect.Effect)
            {
                case CourtStateEffect.None:
                    ValidateNone(effect, result);
                    break;

                case CourtStateEffect.RevealStatement:
                case CourtStateEffect.JumpStatement:
                    ValidateTarget(
                        effect,
                        EffectTargetType.Statement,
                        result);
                    break;

                case CourtStateEffect.RevealTestimony:
                case CourtStateEffect.JumpTestimony:
                    ValidateTarget(
                        effect,
                        EffectTargetType.Testimony,
                        result);
                    break;

                case CourtStateEffect.RevealWitness:
                case CourtStateEffect.JumpWitness:
                    ValidateTarget(
                        effect,
                        EffectTargetType.Witness,
                        result);
                    break;

                case CourtStateEffect.UnlockEvidence:
                    ValidateTarget(
                        effect,
                        EffectTargetType.Evidence,
                        result);
                    break;

                case CourtStateEffect.ModifyCourtStat:
                    ValidateCourtStat(effect, result);
                    break;

                case CourtStateEffect.ModifyCharacterStat:
                    ValidateCharacterStat(effect, result);
                    break;
            }
        }

        private static void ValidateNone(
            CourtStateEffectData effect,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == EffectTargetType.None,
                ValidationScope.Effect,
                "Effect 'None' must use TargetType.None.");

            ValidationUtility.Ensure(
                result,
                !effect.HasTarget,
                ValidationScope.Effect,
                "Effect 'None' must not define TargetId.");
        }

        private static void ValidateTarget(
            CourtStateEffectData effect,
            EffectTargetType expectedTarget,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == expectedTarget,
                ValidationScope.Effect,
                $"'{effect.Effect}' requires TargetType.{expectedTarget}.");

            ValidationUtility.Ensure(
                result,
                effect.HasTarget,
                ValidationScope.Effect,
                $"'{effect.Effect}' requires a TargetId.");
        }

        private static void ValidateCourtStat(
            CourtStateEffectData effect,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == EffectTargetType.None,
                ValidationScope.Effect,
                "ModifyCourtStat must use TargetType.None.");

            ValidationUtility.Ensure(
                result,
                !effect.HasTarget,
                ValidationScope.Effect,
                "ModifyCourtStat must not define TargetId.");
        }

        private static void ValidateCharacterStat(
            CourtStateEffectData effect,
            ValidationResult result)
        {
            ValidationUtility.Ensure(
                result,
                effect.TargetType == EffectTargetType.Character,
                ValidationScope.Effect,
                "ModifyCharacterStat must use TargetType.Character.");

            ValidationUtility.Ensure(
                result,
                effect.HasTarget,
                ValidationScope.Effect,
                "ModifyCharacterStat requires a TargetId.");
        }
    }
}
