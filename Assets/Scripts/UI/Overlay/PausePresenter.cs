using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Verdict.Input;
using Verdict.Systems;

namespace Verdict.UI.Overlay
{
    public sealed class PausePresenter : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField]
        private GameObject pausePanel;

        private CourtroomController courtroomController;

        private VerdictInputActions inputActions;

        public event Action PauseOpened;
        public event Action PauseClosed;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            Hide();
        }

        public void Bind(
            CourtroomController controller,
            VerdictInputActions inputActions)
        {
            Unbind();

            courtroomController = controller;
            this.inputActions = inputActions;

            if (courtroomController == null)
            {
                return;
            }

            if (this.inputActions != null)
            {
                this.inputActions.Player.Pause.performed +=
                    HandlePauseInput;
            }
        }

        public void Unbind()
        {
            if (inputActions != null)
            {
                inputActions.Player.Pause.performed -=
                    HandlePauseInput;
                inputActions = null;
            }

            courtroomController = null;
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
                Hide();
                courtroomController.Resume();
                return;
            }

            if (!courtroomController.CanInteract)
            {
                return;
            }

            Show();
            courtroomController.Pause();
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
        }
    }
}
