using System;
using System.Collections.Generic;
using Verdict.Data.Cases;
using Verdict.Runtime;

namespace Verdict.Systems
{
    public sealed class CaseSessionManager
    {
        public event Action<CaseRuntime> CaseLoaded;
        public event Action CaseUnloaded;

        public CaseRuntime CurrentCase { get; private set; }

        public bool HasActiveCase => CurrentCase != null;

        // Load a case and optionally apply persisted revealed statement IDs.
        // revealedStatementIds is intended for restore from save: it will be
        // added to CourtStateRuntime.RevealedStatementIds and will also set
        // corresponding StatementRuntime.IsVisible = true so gameplay reflects saved state.
        public void LoadCase(CaseData caseData, IEnumerable<string> revealedStatementIds = null)
        {
            // ArgumentNullException.ThrowIfNull(caseData);
            if (caseData == null)
            {
                throw new ArgumentNullException(nameof(caseData));
            }

            if (HasActiveCase)
            {
                throw new InvalidOperationException(
                    "A case is already loaded.");
            }

            CurrentCase = RuntimeFactory.Create(caseData);

            // Apply persisted revealed IDs (if any) to both CourtStateRuntime and StatementRuntime.
            if (revealedStatementIds != null)
            {
                foreach (string id in revealedStatementIds)
                {
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    try
                    {
                        CurrentCase.CourtState.RevealStatement(id);
                    }
                    catch (ArgumentException)
                    {
                        // ignore invalid ids coming from older save data
                    }

                    // Set runtime visibility if statement exists in the runtime model.
                    foreach (var witness in CurrentCase.Witnesses)
                    {
                        foreach (var testimony in witness.Testimonies)
                        {
                            foreach (var statement in testimony.Statements)
                            {
                                if (statement.Data.Id == id)
                                {
                                    statement.IsVisible = true;
                                    goto NextId; // found, move to next saved id
                                }
                            }
                        }
                    }

                NextId:;
                }
            }

            CaseLoaded?.Invoke(CurrentCase);
        }

        public void UnloadCase()
        {
            if (!HasActiveCase)
            {
                return;
            }

            CurrentCase = null;

            CaseUnloaded?.Invoke();
        }

        public void RestartCase()
        {
            if (!HasActiveCase)
            {
                throw new InvalidOperationException(
                    "No active case to restart.");
            }

            CurrentCase = RuntimeFactory.Create(CurrentCase.Data);

            CaseLoaded?.Invoke(CurrentCase);
        }
    }
}
