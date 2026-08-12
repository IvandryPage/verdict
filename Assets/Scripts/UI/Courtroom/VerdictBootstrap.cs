using System;
using UnityEngine;
using Verdict.Data.Cases;
using Verdict.Input;
using Verdict.Presentation;
using Verdict.Systems;
using Verdict.Systems.Presentation;
using Verdict.UI.Evidence;
using Verdict.UI.Narrative;
using Verdict.UI.Overlay;

namespace Verdict
{
    public sealed class VerdictBootstrap : MonoBehaviour
    {
        [Header("Case")]
        [SerializeField]
        private CaseData caseData;

        [Header("UI")]
        [SerializeField]
        private NarrativePresenter narrativePresenter;

        [SerializeField]
        private EndingPresenter endingPresenter;

        [SerializeField]
        private PausePresenter pausePresenter;

        [SerializeField]
        private EvidenceUnlockedPresenter evidenceUnlockedPresenter;

        [Header("Presentation")]
        [SerializeField]
        private CourtroomCameraRig courtroomCameraRig;

        private CaseSessionManager caseSessionManager;
        private CourtroomController courtroomController;
        private CourtroomCameraController courtroomCameraController;
        private Verdict.Systems.CourtroomEventResolver courtroomEventResolver;
        private VerdictInputActions inputActions;

        public CaseSessionManager CaseSessionManager =>
            caseSessionManager;

        public CourtroomController CourtroomController =>
            courtroomController;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            BeginCase();
        }

        private void Initialize()
        {
            if (caseData == null)
            {
                throw new InvalidOperationException(
                    "VerdictBootstrap requires a CaseData reference.");
            }

            caseSessionManager =
                new CaseSessionManager();

            caseSessionManager.LoadCase(caseData);

            courtroomController =
                new CourtroomController(
                    caseSessionManager);

            inputActions = new VerdictInputActions();
            inputActions.Player.Enable();

            if (courtroomCameraRig != null)
            {
                courtroomCameraController =
                    new CourtroomCameraController(
                        courtroomController);

                courtroomCameraController.Bind();
                courtroomCameraRig.Bind(
                    courtroomCameraController);
            }

            courtroomEventResolver =
                new CourtroomEventResolver(
                    courtroomController,
                    courtroomCameraController);

            courtroomEventResolver.Bind();

            if (narrativePresenter != null)
            {
                narrativePresenter.Bind(
                    courtroomController);
            }

            if (endingPresenter != null)
            {
                endingPresenter.Bind(
                    courtroomController);
            }

            if (pausePresenter != null)
            {
                pausePresenter.Bind(
                    courtroomController,
                    inputActions);
            }

            if (evidenceUnlockedPresenter != null)
            {
                evidenceUnlockedPresenter.Bind(
                    courtroomController);
            }
        }

        private void BeginCase()
        {
            if (courtroomController == null)
            {
                Debug.LogError(
                    "Cannot begin case: CourtroomController is null.",
                    this);

                return;
            }

            try
            {
                courtroomController.BeginCase();
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this);
            }
        }
    }
}
