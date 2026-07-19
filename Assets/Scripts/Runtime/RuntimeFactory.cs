using System;
using System.Collections.Generic;
using System.Linq;
using Verdict.Data.Cases;
using Verdict.Data.Narrative;
using Verdict.Data.Evidence;
using Verdict.Systems.Validation;

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

            ValidationResult validation =
                RuntimeValidator.Validate(data);

            if (!validation.IsValid)
            {
                throw new ValidationException(validation);
            }

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

            IReadOnlyDictionary<string, string> statementNodeIds =
                BuildStatementNodeLookup(data);

            return new CaseRuntime(
                data,
                evidence,
                witnesses,
                courtState,
                statementsById,
                testimoniesById,
                witnessesById,
                statementNodeIds);
        }

        /// <summary>
        /// Scans the case's narrative graph for StatementNodeData and
        /// builds a statementId -> nodeId lookup. This is the only place
        /// gameplay and narrative are connected.
        /// </summary>
        private static IReadOnlyDictionary<string, string> BuildStatementNodeLookup(
            CaseData data)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            IReadOnlyList<NarrativeNodeData> nodes =
                data.Narrative?.Nodes;

            if (nodes == null)
            {
                return map;
            }

            foreach (NarrativeNodeData node in nodes)
            {
                if (node is not StatementNodeData statementNode)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(statementNode.StatementId))
                {
                    continue;
                }

                if (!map.ContainsKey(statementNode.StatementId))
                {
                    map.Add(statementNode.StatementId, statementNode.NodeId);
                }
            }

            return map;
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
    }
}
