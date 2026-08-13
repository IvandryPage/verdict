using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Runtime;

namespace Verdict.Systems.Save
{
    public sealed class GameSaveService
    {
        private readonly string saveDirectoryPath;
        private readonly string saveFileName;

        public GameSaveService(string fileName = "savegame.json")
        {
            saveDirectoryPath = Path.Combine(
                Application.persistentDataPath,
                "VerdictSaves");
            saveFileName = fileName;

            Directory.CreateDirectory(saveDirectoryPath);
        }

        public bool Save(CaseSession session)
        {
            if (session == null)
            {
                return false;
            }

            GameSaveData data = BuildSaveData(session);
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(saveDirectoryPath, saveFileName);
            File.WriteAllText(path, json);
            return true;
        }

        public GameSaveData Load()
        {
            string path = Path.Combine(saveDirectoryPath, saveFileName);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<GameSaveData>(json);
        }

        public bool Restore(CaseSession session, GameSaveData data)
        {
            if (session == null || data == null)
            {
                return false;
            }

            CaseRuntime runtime = session.Runtime;
            if (runtime == null)
            {
                return false;
            }

            foreach (string statementId in data.RevealedStatementIds)
            {
                if (string.IsNullOrWhiteSpace(statementId))
                {
                    continue;
                }

                runtime.CourtState.RevealStatement(statementId);
                if (runtime.TryGetStatement(statementId, out StatementRuntime statement))
                {
                    statement.IsVisible = true;
                }
            }

            foreach (StringBoolEntry entry in data.StatementVisibility)
            {
                if (string.IsNullOrWhiteSpace(entry?.Key))
                {
                    continue;
                }

                if (runtime.TryGetStatement(entry.Key, out StatementRuntime statement))
                {
                    statement.IsVisible = entry.Value;
                }
            }

            foreach (StringBoolEntry entry in data.EvidenceUnlocked)
            {
                if (string.IsNullOrWhiteSpace(entry?.Key))
                {
                    continue;
                }

                foreach (EvidenceRuntime evidence in runtime.Evidence)
                {
                    if (evidence.Data.Id == entry.Key)
                    {
                        evidence.IsUnlocked = entry.Value;
                        break;
                    }
                }
            }

            foreach (StringBoolEntry entry in data.EvidenceVisibility)
            {
                if (string.IsNullOrWhiteSpace(entry?.Key))
                {
                    continue;
                }

                foreach (EvidenceRuntime evidence in runtime.Evidence)
                {
                    if (evidence.Data.Id == entry.Key)
                    {
                        evidence.IsCollected = entry.Value;
                        break;
                    }
                }
            }

            foreach (StatSaveEntry stat in data.CourtStats)
            {
                if (Enum.TryParse(stat.StatName, out CourtStat courtStat))
                {
                    runtime.CourtState.ModifyCourtStat(
                        courtStat,
                        stat.Value,
                        StatOperation.Set);
                }
            }

            RestoreFlowToValidStatement(session, data);

            if (session.NarrativeCoordinator != null)
            {
                if (!session.NarrativeCoordinator.HasActiveNarrative)
                {
                    session.NarrativeCoordinator.Play(session.Runtime.Data.Narrative);
                }

                if (!string.IsNullOrWhiteSpace(data.CurrentNarrativeNodeId) &&
                    !session.NarrativeCoordinator.RestoreRuntimePosition(
                        data.CurrentNarrativeNodeId,
                        data.CurrentNarrativeEntryIndex))
                {
                    session.NarrativeCoordinator.Play(session.Runtime.Data.Narrative);
                }
            }

            return true;
        }

        public bool DeleteSave()
        {
            string path = Path.Combine(saveDirectoryPath, saveFileName);
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }

        public bool SaveExists()
        {
            string path = Path.Combine(saveDirectoryPath, saveFileName);
            return File.Exists(path);
        }

        private static void RestoreFlowToValidStatement(
            CaseSession session,
            GameSaveData data)
        {
            if (session == null || session.Flow == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(data.CurrentStatementId) &&
                session.Flow.TryMoveToStatement(data.CurrentStatementId))
            {
                return;
            }

            foreach (StatementRuntime statement in session.Runtime.StatementsById.Values)
            {
                if (statement != null && statement.IsVisible)
                {
                    session.Flow.TryMoveToStatement(statement.Data.Id);
                    return;
                }
            }

            session.Flow.Reset();
        }

        private static GameSaveData BuildSaveData(CaseSession session)
        {
            GameSaveData data = new GameSaveData();

            if (session == null)
            {
                return data;
            }

            CaseRuntime runtime = session.Runtime;
            if (runtime == null)
            {
                return data;
            }

            data.CaseId = runtime.Data?.Id;
            data.CurrentStatementId = session.Flow.CurrentStatement?.Data?.Id;
            data.CurrentWitnessId = session.Flow.CurrentWitness?.Data?.Id;
            data.CurrentTestimonyId = session.Flow.CurrentTestimony?.Data?.Id;

            if (session.NarrativeCoordinator != null &&
                session.NarrativeCoordinator.HasActiveNarrative)
            {
                data.CurrentNarrativeNodeId = session.NarrativeCoordinator.CurrentNodeId;
                data.CurrentNarrativeEntryIndex = session.NarrativeCoordinator.CurrentEntryIndex;
            }

            data.IsNarrativeFinished = session.NarrativeCoordinator == null ||
                !session.NarrativeCoordinator.HasActiveNarrative;

            if (runtime.CourtState != null)
            {
                foreach (CourtStat stat in Enum.GetValues(typeof(CourtStat)))
                {
                    data.CourtStats.Add(new StatSaveEntry
                    {
                        StatName = stat.ToString(),
                        Value = runtime.CourtState.GetCourtStat(stat)
                    });
                }

                data.RevealedStatementIds.AddRange(runtime.CourtState.RevealedStatementIds);
                data.RevealedTestimonyIds.AddRange(runtime.CourtState.RevealedTestimonyIds);
                data.UnlockedEvidenceIds.AddRange(runtime.CourtState.UnlockedEvidenceIds);
            }

            foreach (StatementRuntime statement in runtime.StatementsById.Values)
            {
                data.StatementVisibility.Add(new StringBoolEntry
                {
                    Key = statement.Data.Id,
                    Value = statement.IsVisible
                });
            }

            foreach (EvidenceRuntime evidence in runtime.Evidence)
            {
                data.EvidenceVisibility.Add(new StringBoolEntry
                {
                    Key = evidence.Data.Id,
                    Value = evidence.IsCollected
                });

                data.EvidenceUnlocked.Add(new StringBoolEntry
                {
                    Key = evidence.Data.Id,
                    Value = evidence.IsUnlocked
                });
            }

            return data;
        }
    }
}
