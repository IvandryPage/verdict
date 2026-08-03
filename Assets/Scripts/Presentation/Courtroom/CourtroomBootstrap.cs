using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Runtime;
using Verdict.Systems;
using Verdict.Systems.Evaluation;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Scene-level entry point for a courtroom scene.
    /// Builds the runtime, session, and controller, then exposes the
    /// controller to the rest of the presentation layer.
    /// </summary>
    public sealed class CourtroomBootstrap : MonoBehaviour
    {
        [Header("Case")]
        [SerializeField] private CaseData caseData;

        [Header("Startup")]
        [SerializeField] private bool beginCaseOnAwake = true;

        public CourtroomController Controller { get; private set; }
        public CaseRuntime Runtime { get; private set; }

        private CaseSessionManager sessionManager;

        private void Awake()
        {
            if (caseData == null)
            {
                Debug.LogError($"{nameof(CourtroomBootstrap)}: No CaseData assigned.");
                enabled = false;
                return;
            }

            sessionManager = new CaseSessionManager();

            sessionManager.LoadCase(caseData);

            Controller = new CourtroomController(sessionManager);
        }

        private void Start()
        {
            if (beginCaseOnAwake)
            {
                Controller.BeginCase();
            }
        }
    }
}
