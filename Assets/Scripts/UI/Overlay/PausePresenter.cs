using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Verdict.Input;
using Verdict.Systems;

namespace Verdict.UI.Overlay
{
    public sealed class PausePresenter : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField]
        private GameObject pausePanel;

        [Header("Controls")]
        [SerializeField]
        private Button pauseButton;

        [SerializeField]
        private Button resumeButton;

        private CourtroomController courtroomController;

        private VerdictInputActions inputActions;

        public event Action PauseOpened;
        public event Action PauseClosed;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            inputActions = new VerdictInputActions();
            inputActions.Player.Pause.performed += HandlePauseInput;
            inputActions.Player.Enable();

            if (pauseButton != null)
            {
                pauseButton.clicked += HandlePausePressed;
            }

            if (resumeButton != null)
            {
                resumeButton.clicked += HandleResumePressed;
            }

            Hide();
        }

        public void Bind(
            CourtroomController controller)
        {
            Unbind();

            courtroomController = controller;

            if (courtroomController == null)
            {
                return;
            }
        }

        public void Unbind()
        {
            courtroomController = null;
        }

        private void HandlePausePressed()
        {
            if (courtroomController == null)
            {
                return;
            }

            if (!courtroomController.CanInteract)
            {
                return;
            }

            Show();

            courtroomController.Pause();
        }

        private void HandlePauseInput(
            InputAction.CallbackContext context)
        {
            if (!context.performed || courtroomController == null)
            {
                return;
            }

            if (IsPaused)
            {
                HandleResumePressed();
                return;
            }

            if (!courtroomController.CanInteract)
            {
                return;
            }

            Show();
            courtroomController.Pause();
        }

        private void HandleResumePressed()
        {
            if (courtroomController == null)
            {
                return;
            }

            Hide();

            courtroomController.Resume();
        }

        public void Show()
        {
            IsPaused = true;

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }

            PauseOpened?.Invoke();
        }

        public void Hide()
        {
            IsPaused = false;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            PauseClosed?.Invoke();
        }

        private void OnDestroy()
        {
            Unbind();

            if (pauseButton != null)
            {
                pauseButton.clicked -= HandlePausePressed;
            }

            if (resumeButton != null)
            {
                resumeButton.clicked -= HandleResumePressed;
            }

            if (inputActions != null)
            {
                inputActions.Player.Pause.performed -= HandlePauseInput;
                inputActions.Dispose();
                inputActions = null;
            }
        }
    }
}
