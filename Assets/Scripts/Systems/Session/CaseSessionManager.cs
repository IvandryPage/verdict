using System;
using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Runtime;
using Verdict.Systems.Evaluation;
using Verdict.Systems.Save;

namespace Verdict.Systems
{
    public sealed class CaseSessionManager
    {
        public event Action<CaseSession> SessionLoaded;
        public event Action SessionUnloaded;

        public CaseSession CurrentSession { get; private set; }

        public bool HasActiveSession =>
            CurrentSession != null;

        public void LoadCase(
            CaseData caseData,
            IEnumerable<string> revealedStatementIds = null)
        {
            if (caseData == null)
            {
                throw new ArgumentNullException(nameof(caseData));
            }

            if (HasActiveSession)
            {
                throw new InvalidOperationException(
                    "A case is already loaded.");
            }

            CaseRuntime runtime =
                RuntimeFactory.Create(caseData);

            CourtroomFlow flow =
                new CourtroomFlow(runtime);

            NarrativeRunner narrativeRunner =
                new NarrativeRunner(runtime);

            NarrativeCoordinator narrativeCoordinator =
                new NarrativeCoordinator(narrativeRunner);

            ResolverEngine resolverEngine =
                new ResolverEngine(flow);

            CourtStateEffectProcessor effectProcessor =
                new CourtStateEffectProcessor(runtime);

            CurrentSession =
                new CaseSession(
                    runtime,
                    flow,
                    narrativeCoordinator,
                    resolverEngine,
                    effectProcessor);

            if (revealedStatementIds != null)
            {
                foreach (string id in revealedStatementIds)
                {
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    try
                    {
                        CurrentSession.Runtime.CourtState
                            .RevealStatement(id);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }

                    if (CurrentSession.Runtime.TryGetStatement(
                        id,
                        out StatementRuntime statement))
                    {
                        statement.IsVisible = true;
                    }
                }
            }

            SessionLoaded?.Invoke(CurrentSession);
        }

        public void UnloadCase()
        {
            if (!HasActiveSession)
            {
                return;
            }

            CurrentSession = null;

            SessionUnloaded?.Invoke();
        }

        public void RestartCase()
        {
            if (!HasActiveSession)
            {
                throw new InvalidOperationException(
                    "No active case.");
            }

            CaseData data =
                CurrentSession.Runtime.Data;

            UnloadCase();
            LoadCase(data);
        }

        public bool SaveCurrentCase(
            string fileName = "savegame.json")
        {
            if (!HasActiveSession)
            {
                return false;
            }

            GameSaveService service = new GameSaveService(fileName);
            return service.Save(CurrentSession);
        }

        public GameSaveData LoadSavedGame(
            string fileName = "savegame.json")
        {
            GameSaveService service = new GameSaveService(fileName);
            return service.Load();
        }

        public bool RestoreFromSave(
            GameSaveData saveData,
            CaseData caseData,
            string fileName = "savegame.json")
        {
            if (saveData == null || caseData == null)
            {
                return false;
            }

            if (HasActiveSession)
            {
                UnloadCase();
            }

            LoadCase(caseData);

            GameSaveService service = new GameSaveService(fileName);
            return service.Restore(CurrentSession, saveData);
        }

        public bool TryLoadSavedCase(
            CaseData caseData,
            string fileName = "savegame.json")
        {
            if (caseData == null)
            {
                return false;
            }

            GameSaveData saveData = LoadSavedGame(fileName);
            if (saveData == null)
            {
                return false;
            }

            return RestoreFromSave(saveData, caseData, fileName);
        }
    }
}
