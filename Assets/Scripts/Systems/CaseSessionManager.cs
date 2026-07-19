using System;
using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Runtime;

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
                new NarrativeRunner(runtime.CourtState);

            NarrativeCoordinator narrativeCoordinator =
                new NarrativeCoordinator(narrativeRunner);

            EvaluationSystem evaluationSystem =
                new EvaluationSystem(flow);

            CourtStateEffectProcessor effectProcessor =
                new CourtStateEffectProcessor(runtime);

            CurrentSession =
                new CaseSession(
                    runtime,
                    flow,
                    narrativeCoordinator,
                    evaluationSystem,
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
    }
}
