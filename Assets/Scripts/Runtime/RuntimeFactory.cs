using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Data.Evidence;

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

            CourtStateRuntime courtState = CreateCourtState(data);

            ApplyCharacterOverrides(witnesses, data);

            IReadOnlyDictionary<string, StatementRuntime> statementsById =
                BuildStatementLookup(witnesses);

            IReadOnlyDictionary<string, TestimonyRuntime> testimoniesById =
                BuildTestimonyLookup(witnesses);

            IReadOnlyDictionary<string, WitnessRuntime> witnessesById =
                BuildWitnessLookup(witnesses);

            return new CaseRuntime(
                data,
                evidence,
                witnesses,
                courtState,
                statementsById,
                testimoniesById,
                witnessesById);
        }

        private static IReadOnlyList<EvidenceRuntime> CreateEvidence(
            CaseData data)
        {
            return data.Evidence
                .Select(entry => new EvidenceRuntime(entry.Evidence)
                {
                    IsUnlocked = entry.InitiallyUnlocked
                })
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
            IReadOnlyList<TestimonyRuntime> testimonies =
                witness.Testimonies
                    .Select(CreateTestimony)
                    .ToList();

            return new WitnessRuntime(
                witness,
                testimonies,
                new CharacterRuntime(witness.Character))
            {
                IsVisible = witness.InitiallyVisible
            };
        }

        private static TestimonyRuntime CreateTestimony(
            TestimonyData testimony)
        {
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
            return new StatementRuntime(statement)
            {
                IsVisible = statement.InitiallyVisible
            };
        }

        private static CourtStateRuntime CreateCourtState(CaseData data)
        {
            var courtState = new CourtStateRuntime();

            if (data.CourtSetup != null)
            {
                courtState.ModifyCourtStat(CourtStat.JudgeTrust, data.CourtSetup.JudgeTrust, StatOperation.Set);
                courtState.ModifyCourtStat(CourtStat.Penalty, data.CourtSetup.Penalty, StatOperation.Set);
                courtState.ModifyCourtStat(CourtStat.PublicOpinion, data.CourtSetup.PublicOpinion, StatOperation.Set);
                courtState.ModifyCourtStat(CourtStat.JuryTrust, data.CourtSetup.JuryOpinion, StatOperation.Set);
                courtState.ModifyCourtStat(CourtStat.StoryProgress, data.CourtSetup.StoryProgress, StatOperation.Set);
                courtState.ModifyCourtStat(CourtStat.CaseProgress, data.CourtSetup.CaseProgress, StatOperation.Set);
            }

            return courtState;
        }

        private static void ApplyCharacterOverrides(
            IReadOnlyList<WitnessRuntime> witnesses,
            CaseData data)
        {
            if (data.CharacterOverrides == null || data.CharacterOverrides.Count == 0)
            {
                return;
            }

            var overridesByCharacter = data.CharacterOverrides
                .Where(o => o.Character != null)
                .ToDictionary(o => o.Character.Id, StringComparer.Ordinal);

            foreach (WitnessRuntime witness in witnesses)
            {
                string characterId = witness.Character.Data.Id;
                if (overridesByCharacter.TryGetValue(characterId, out CharacterOverrideData overrideData))
                {
                    ApplyCharacterOverride(witness.Character, overrideData);
                }
            }
        }

        private static void ApplyCharacterOverride(
            CharacterRuntime character,
            CharacterOverrideData overrideData)
        {
            if (overrideData.OverrideCredibility)
                character.ModifyCharacterStat(CharacterStat.Credibility, overrideData.Credibility, StatOperation.Set);

            if (overrideData.OverrideTrust)
                character.ModifyCharacterStat(CharacterStat.Trust, overrideData.Trust, StatOperation.Set);

            if (overrideData.OverrideStress)
                character.ModifyCharacterStat(CharacterStat.Stress, overrideData.Stress, StatOperation.Set);

            if (overrideData.OverrideFear)
                character.ModifyCharacterStat(CharacterStat.Fear, overrideData.Fear, StatOperation.Set);

            if (overrideData.OverrideCooperation)
                character.ModifyCharacterStat(CharacterStat.Cooperation, overrideData.Cooperation, StatOperation.Set);

            if (overrideData.OverrideAffinity)
                character.ModifyCharacterStat(CharacterStat.Affinity, overrideData.Affinity, StatOperation.Set);
        }

        private static IReadOnlyDictionary<string, StatementRuntime> BuildStatementLookup(
            IReadOnlyList<WitnessRuntime> witnesses)
        {
            var map = new Dictionary<string, StatementRuntime>(StringComparer.Ordinal);
            foreach (WitnessRuntime witness in witnesses)
            {
                foreach (TestimonyRuntime testimony in witness.Testimonies)
                {
                    foreach (StatementRuntime statement in testimony.Statements)
                    {
                        if (string.IsNullOrWhiteSpace(statement.Data.Id))
                        {
                            continue;
                        }

                        if (!map.ContainsKey(statement.Data.Id))
                        {
                            map.Add(statement.Data.Id, statement);
                        }
                    }
                }
            }

            return map;
        }

        private static IReadOnlyDictionary<string, TestimonyRuntime> BuildTestimonyLookup(
            IReadOnlyList<WitnessRuntime> witnesses)
        {
            var map = new Dictionary<string, TestimonyRuntime>(StringComparer.Ordinal);
            foreach (WitnessRuntime witness in witnesses)
            {
                foreach (TestimonyRuntime testimony in witness.Testimonies)
                {
                    if (string.IsNullOrWhiteSpace(testimony.Data.Id))
                    {
                        continue;
                    }

                    if (!map.ContainsKey(testimony.Data.Id))
                    {
                        map.Add(testimony.Data.Id, testimony);
                    }
                }
            }

            return map;
        }

        private static IReadOnlyDictionary<string, WitnessRuntime> BuildWitnessLookup(
            IReadOnlyList<WitnessRuntime> witnesses)
        {
            var map = new Dictionary<string, WitnessRuntime>(StringComparer.Ordinal);
            foreach (WitnessRuntime witness in witnesses)
            {
                string witnessId = witness.Data.Id;
                if (string.IsNullOrWhiteSpace(witnessId))
                {
                    continue;
                }

                if (!map.ContainsKey(witnessId))
                {
                    map.Add(witnessId, witness);
                }
            }

            return map;
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
                data.Evidence.Count > 0,
                $"Case '{data.name}' must contain at least one evidence entry.");

            Ensure(
                data.Witnesses != null,
                $"Case '{data.name}' has no witness list.");

            Ensure(
                data.Witnesses.All(w => w != null),
                $"Case '{data.name}' contains a null witness entry.");

            Ensure(
                data.Witnesses.Count > 0,
                $"Case '{data.name}' must contain at least one witness.");

            Ensure(
                data.Truth != null,
                $"Case '{data.name}' has no truth database.");

            Ensure(
                data.Endings != null,
                $"Case '{data.name}' has no endings list.");

            Ensure(
                data.Truth.Facts != null,
                $"Case '{data.name}' has no truth facts list.");

            var evidenceIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (EvidenceEntryData evidenceEntry in data.Evidence)
            {
                ValidateEvidenceEntry(evidenceEntry, evidenceIds);
            }

            var factIds = BuildFactIdLookup(data.Truth);

            var witnessIds = new HashSet<string>(StringComparer.Ordinal);
            var testimonyIds = new HashSet<string>(StringComparer.Ordinal);
            var statementIds = new HashSet<string>(StringComparer.Ordinal);
            var claimIds = new HashSet<string>(StringComparer.Ordinal);
            var endingIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (EndingData ending in data.Endings)
            {
                Ensure(
                    ending != null,
                    $"Case '{data.name}' contains a null ending entry.");

                Ensure(
                    !string.IsNullOrWhiteSpace(ending.Id),
                    $"A case ending in '{data.name}' has no ID.");

                Ensure(
                    endingIds.Add(ending.Id),
                    $"Duplicate ending ID '{ending.Id}' found.");
            }

            foreach (WitnessData witness in data.Witnesses)
            {
                ValidateWitness(
                    witness,
                    witnessIds,
                    testimonyIds,
                    statementIds,
                    claimIds,
                    evidenceIds,
                    factIds);
            }

            ValidateReferences(data, statementIds, testimonyIds, witnessIds, evidenceIds, endingIds);
        }

        private static void ValidateEvidenceEntry(
            EvidenceEntryData evidenceEntry,
            HashSet<string> evidenceIds)
        {
            Ensure(
                evidenceEntry != null,
                "Encountered a null EvidenceEntryData.");

            Ensure(
                evidenceEntry.Evidence != null,
                "An evidence entry references no EvidenceData.");

            ValidateEvidence(evidenceEntry.Evidence, evidenceIds);
        }

        private static void ValidateWitness(
            WitnessData witness,
            HashSet<string> witnessIds,
            HashSet<string> testimonyIds,
            HashSet<string> statementIds,
            HashSet<string> claimIds,
            HashSet<string> evidenceIds,
            HashSet<string> factIds)
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

            Ensure(
                !string.IsNullOrWhiteSpace(witness.Id),
                "A witness has no ID.");

            Ensure(
                witnessIds.Add(witness.Id),
                $"Duplicate witness ID '{witness.Id}' found.");

            foreach (TestimonyData testimony in witness.Testimonies)
            {
                ValidateTestimony(
                    testimony,
                    witness,
                    testimonyIds,
                    statementIds,
                    claimIds,
                    evidenceIds,
                    factIds);
            }
        }

        private static void ValidateTestimony(
            TestimonyData testimony,
            WitnessData witness,
            HashSet<string> testimonyIds,
            HashSet<string> statementIds,
            HashSet<string> claimIds,
            HashSet<string> evidenceIds,
            HashSet<string> factIds)
        {
            Ensure(
                testimony != null,
                "Encountered a null TestimonyData.");

            Ensure(
                testimony.Statements != null,
                $"Testimony '{testimony.Title}' in witness '{witness.Id}' has no statement list.");

            Ensure(
                testimony.Statements.All(s => s != null),
                $"Testimony '{testimony.Title}' in witness '{witness.Id}' contains a null statement entry.");

            Ensure(
                testimony.Statements.Count > 0,
                $"Testimony '{testimony.Title}' in witness '{witness.Id}' must contain at least one statement.");

            Ensure(
                !string.IsNullOrWhiteSpace(testimony.Id),
                $"A testimony in witness '{witness.Id}' has no ID.");

            Ensure(
                testimonyIds.Add(testimony.Id),
                $"Duplicate testimony ID '{testimony.Id}' found.");

            foreach (StatementData statement in testimony.Statements)
            {
                ValidateStatement(
                    statement,
                    testimony,
                    witness,
                    statementIds,
                    claimIds,
                    evidenceIds,
                    factIds);
            }
        }

        private static void ValidateStatement(
            StatementData statement,
            TestimonyData testimony,
            WitnessData witness,
            HashSet<string> statementIds,
            HashSet<string> claimIds,
            HashSet<string> evidenceIds,
            HashSet<string> factIds)
        {
            Ensure(
                statement != null,
                "Encountered a null StatementData.");

            Ensure(
                statement.Claims != null,
                $"Statement '{statement.Id}' in testimony '{testimony.Title}' has no claims list.");

            Ensure(
                statement.Claims.All(c => c != null),
                $"Statement '{statement.Id}' in testimony '{testimony.Title}' contains a null claim entry.");

            Ensure(
                !string.IsNullOrWhiteSpace(statement.Id),
                $"A statement in testimony '{testimony.Title}' has no ID.");

            Ensure(
                statementIds.Add(statement.Id),
                $"Duplicate statement ID '{statement.Id}' found.");

            if (!string.IsNullOrWhiteSpace(statement.NextStatementId))
            {
                Ensure(
                    statement.NextStatementId != statement.Id,
                    $"Statement '{statement.Id}' cannot use itself as NextStatementId.");
            }

            foreach (ClaimData claim in statement.Claims)
            {
                ValidateClaim(
                    claim,
                    statement,
                    testimony,
                    witness,
                    claimIds,
                    evidenceIds,
                    factIds);
            }
        }

        private static void ValidateClaim(
            ClaimData claim,
            StatementData statement,
            TestimonyData testimony,
            WitnessData witness,
            HashSet<string> claimIds,
            HashSet<string> evidenceIds,
            HashSet<string> factIds)
        {
            Ensure(
                claim != null,
                "Encountered a null ClaimData.");

            Ensure(
                claim.EvaluationRules != null,
                $"Claim '{claim.Id}' in statement '{statement.Id}' has no evaluation rules list.");

            Ensure(
                claim.EvaluationRules.All(r => r != null),
                $"Claim '{claim.Id}' in statement '{statement.Id}' contains a null evaluation rule entry.");

            Ensure(
                !string.IsNullOrWhiteSpace(claim.Id),
                $"A claim in statement '{statement.Id}' has no ID.");

            Ensure(
                claimIds.Add(claim.Id),
                $"Duplicate claim ID '{claim.Id}' found.");

            Ensure(
                !string.IsNullOrWhiteSpace(claim.FactId),
                $"Claim '{claim.Id}' in statement '{statement.Id}' references no fact.");

            Ensure(
                factIds.Contains(claim.FactId),
                $"Claim '{claim.Id}' in statement '{statement.Id}' references unknown fact '{claim.FactId}'.");

            foreach (EvaluationRuleData rule in claim.EvaluationRules)
            {
                ValidateEvaluationRule(
                    rule,
                    claim,
                    statement,
                    testimony,
                    witness,
                    evidenceIds);
            }
        }

        private static void ValidateEvaluationRule(
            EvaluationRuleData rule,
            ClaimData claim,
            StatementData statement,
            TestimonyData testimony,
            WitnessData witness,
            HashSet<string> evidenceIds)
        {
            Ensure(
                rule != null,
                "Encountered a null EvaluationRuleData.");

            Ensure(
                rule.SuccessEffects != null,
                $"Evaluation rule in claim '{claim.Id}' has a null SuccessEffects list.");

            Ensure(
                rule.SuccessEffects.All(e => e != null),
                $"Evaluation rule in claim '{claim.Id}' contains a null CourtStateEffectData in SuccessEffects.");

            Ensure(
                rule.FailureEffects != null,
                $"Evaluation rule in claim '{claim.Id}' has a null FailureEffects list.");

            Ensure(
                rule.FailureEffects.All(e => e != null),
                $"Evaluation rule in claim '{claim.Id}' contains a null CourtStateEffectData in FailureEffects.");

            if (rule.EvaluationType == EvaluationType.PresentEvidence)
            {
                Ensure(
                    rule.RequiredEvidence != null,
                    $"PresentEvidence rule in claim '{claim.Id}' must reference required evidence.");

                Ensure(
                    evidenceIds.Contains(rule.RequiredEvidence.Id),
                    $"PresentEvidence rule in claim '{claim.Id}' references evidence '{rule.RequiredEvidence?.Id}' that is not part of the case.");
            }

            foreach (CourtStateEffectData effect in rule.SuccessEffects)
            {
                ValidateEffect(effect, claim, statement, testimony, witness);
            }

            foreach (CourtStateEffectData effect in rule.FailureEffects)
            {
                ValidateEffect(effect, claim, statement, testimony, witness);
            }
        }

        private static void ValidateEffect(
            CourtStateEffectData effect,
            ClaimData claim,
            StatementData statement,
            TestimonyData testimony,
            WitnessData witness)
        {
            Ensure(
                effect != null,
                $"Evaluation rule in claim '{claim.Id}' contains a null effect.");

            switch (effect.Effect)
            {
                case CourtStateEffect.None:
                    break;

                case CourtStateEffect.RevealStatement:
                    Ensure(
                        !string.IsNullOrWhiteSpace(effect.TargetId),
                        $"RevealStatement effect in claim '{claim.Id}' must reference a statement ID.");
                    break;

                case CourtStateEffect.RevealTestimony:
                case CourtStateEffect.JumpTestimony:
                    Ensure(
                        !string.IsNullOrWhiteSpace(effect.TargetId),
                        $"{effect.Effect} effect in claim '{claim.Id}' must reference a testimony ID.");
                    break;

                case CourtStateEffect.RevealWitness:
                case CourtStateEffect.JumpWitness:
                case CourtStateEffect.ModifyCharacterStat:
                    Ensure(
                        !string.IsNullOrWhiteSpace(effect.TargetId),
                        $"{effect.Effect} effect in claim '{claim.Id}' must reference a witness ID.");
                    break;

                case CourtStateEffect.JumpStatement:
                    Ensure(
                        !string.IsNullOrWhiteSpace(effect.TargetId),
                        $"JumpStatement effect in claim '{claim.Id}' must reference a statement ID.");
                    break;

                case CourtStateEffect.UnlockEvidence:
                    Ensure(
                        !string.IsNullOrWhiteSpace(effect.TargetId),
                        $"UnlockEvidence effect in claim '{claim.Id}' must reference an evidence ID.");
                    break;

                case CourtStateEffect.ModifyCourtStat:
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported effect type '{effect.Effect}' in claim '{claim.Id}'.");
            }
        }

        private static HashSet<string> BuildFactIdLookup(TruthData truth)
        {
            var factIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (FactData fact in truth.Facts)
            {
                Ensure(
                    fact != null,
                    "Encountered a null FactData.");

                Ensure(
                    !string.IsNullOrWhiteSpace(fact.Id),
                    "A fact in the truth database has no ID.");

                Ensure(
                    factIds.Add(fact.Id),
                    $"Duplicate fact ID '{fact.Id}' found in the truth database.");
            }

            return factIds;
        }

        private static void ValidateEvidence(
            EvidenceData evidence,
            HashSet<string> evidenceIds)
        {
            Ensure(
                evidence != null,
                "Encountered a null EvidenceData.");

            Ensure(
                !string.IsNullOrWhiteSpace(evidence.Id),
                "An evidence asset has no ID.");

            Ensure(
                evidenceIds.Add(evidence.Id),
                $"Duplicate evidence ID '{evidence.Id}' found in the case.");
        }

        private static void ValidateReferences(
            CaseData data,
            HashSet<string> statementIds,
            HashSet<string> testimonyIds,
            HashSet<string> witnessIds,
            HashSet<string> evidenceIds,
            HashSet<string> endingIds)
        {
            foreach (WitnessData witness in data.Witnesses)
            {
                foreach (TestimonyData testimony in witness.Testimonies)
                {
                    foreach (StatementData statement in testimony.Statements)
                    {
                        if (!string.IsNullOrWhiteSpace(statement.NextStatementId))
                        {
                            Ensure(
                                statement.NextStatementId != statement.Id,
                                $"Statement '{statement.Id}' cannot use itself as NextStatementId.");

                            Ensure(
                                statementIds.Contains(statement.NextStatementId),
                                $"Statement '{statement.Id}' references unknown NextStatementId '{statement.NextStatementId}'.");
                        }

                        foreach (ClaimData claim in statement.Claims)
                        {
                            foreach (EvaluationRuleData rule in claim.EvaluationRules)
                            {
                                foreach (CourtStateEffectData effect in rule.SuccessEffects)
                                {
                                    ValidateEffectTarget(effect, claim, statement, testimony, witnessIds, testimonyIds, statementIds, evidenceIds, endingIds);
                                }

                                foreach (CourtStateEffectData effect in rule.FailureEffects)
                                {
                                    ValidateEffectTarget(effect, claim, statement, testimony, witnessIds, testimonyIds, statementIds, evidenceIds, endingIds);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateEffectTarget(
            CourtStateEffectData effect,
            ClaimData claim,
            StatementData statement,
            TestimonyData testimony,
            HashSet<string> witnessIds,
            HashSet<string> testimonyIds,
            HashSet<string> statementIds,
            HashSet<string> evidenceIds,
            HashSet<string> endingIds)
        {
            if (effect == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(effect.TargetId))
            {
                return;
            }

            switch (effect.Effect)
            {
                case CourtStateEffect.RevealStatement:
                case CourtStateEffect.JumpStatement:
                    Ensure(
                        statementIds.Contains(effect.TargetId),
                        $"Effect '{effect.Effect}' in claim '{claim.Id}' references unknown statement '{effect.TargetId}'.");
                    break;

                case CourtStateEffect.RevealTestimony:
                case CourtStateEffect.JumpTestimony:
                    Ensure(
                        testimonyIds.Contains(effect.TargetId),
                        $"Effect '{effect.Effect}' in claim '{claim.Id}' references unknown testimony '{effect.TargetId}'.");
                    break;

                case CourtStateEffect.RevealWitness:
                case CourtStateEffect.JumpWitness:
                case CourtStateEffect.ModifyCharacterStat:
                    Ensure(
                        witnessIds.Contains(effect.TargetId),
                        $"Effect '{effect.Effect}' in claim '{claim.Id}' references unknown witness '{effect.TargetId}'.");
                    break;

                case CourtStateEffect.UnlockEvidence:
                    Ensure(
                        evidenceIds.Contains(effect.TargetId),
                        $"Effect '{effect.Effect}' in claim '{claim.Id}' references unknown evidence '{effect.TargetId}'.");
                    break;

                case CourtStateEffect.ModifyCourtStat:
                case CourtStateEffect.None:
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported effect type '{effect.Effect}' in claim '{claim.Id}'.");
            }
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
