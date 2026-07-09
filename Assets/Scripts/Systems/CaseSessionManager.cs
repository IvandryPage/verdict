using System;
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

        public void LoadCase(CaseData caseData)
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
