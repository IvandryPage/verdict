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

        [Header("Canvas Groups")]
        [SerializeField]
        private CanvasGroup pauseGroup;

        private CourtroomController courtroomController;

        private VerdictInputActions inputActions;

        public event Action PauseOpened;
        public event Action PauseClosed;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (pausePanel != null && pauseGroup == null)
            {
                pauseGroup = pausePanel.GetComponent<CanvasGroup>();
                if (pauseGroup == null)
                {
                    pauseGroup = pausePanel.AddComponent<CanvasGroup>();
                }
            }

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
                Debug.Log("[PausePresenter] Bound to input actions");
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
            Debug.Log($"[PausePresenter] Pause input performed. performed={context.performed}, IsPaused={IsPaused}, CanInteract={(courtroomController != null ? courtroomController.CanInteract.ToString() : "null")}");

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

            Show();
            courtroomController.Pause();
        }

        public void Show()
        {
            IsPaused = true;

            if (pauseGroup != null)
            {
                pauseGroup.alpha = 1f;
                pauseGroup.interactable = true;
                pauseGroup.blocksRaycasts = true;
            }
            else if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }

            PauseOpened?.Invoke();
        }

        public void Hide()
        {
            IsPaused = false;

            if (pauseGroup != null)
            {
                pauseGroup.alpha = 0f;
                pauseGroup.interactable = false;
                pauseGroup.blocksRaycasts = false;
            }
            else if (pausePanel != null)
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
