using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Verdict.Data.Cases;
using Verdict.Input;
using Verdict.Systems;
using Verdict.Systems.Save;

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

        [Header("Buttons")]
        [SerializeField]
        private Button saveButton;

        [SerializeField]
        private Button loadButton;

        [SerializeField]
        private Button resumeButton;

        [SerializeField]
        private Button exitToMainMenuButton;

        [SerializeField]
        [Tooltip("Scene name to load when the player exits to main menu.")]
        private string mainMenuSceneName = "MainMenu";

        private CourtroomController courtroomController;
        private CaseSessionManager caseSessionManager;
        private CaseData activeCaseData;

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

            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(SaveGame);
                saveButton.onClick.AddListener(SaveGame);
            }

            if (loadButton != null)
            {
                loadButton.gameObject.SetActive(false);
                loadButton.onClick.RemoveListener(LoadGame);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(ResumeGame);
                resumeButton.onClick.AddListener(ResumeGame);
            }

            if (exitToMainMenuButton != null)
            {
                exitToMainMenuButton.onClick.RemoveListener(ExitToMainMenu);
                exitToMainMenuButton.onClick.AddListener(ExitToMainMenu);
            }

            Hide();
        }

        public void Bind(
            CourtroomController controller,
            VerdictInputActions inputActions,
            CaseSessionManager sessionManager,
            CaseData caseData)
        {
            Unbind();

            courtroomController = controller;
            this.inputActions = inputActions;
            caseSessionManager = sessionManager;
            activeCaseData = caseData;

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

            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(SaveGame);
            }

            if (loadButton != null)
            {
                loadButton.onClick.RemoveListener(LoadGame);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(ResumeGame);
            }

            if (exitToMainMenuButton != null)
            {
                exitToMainMenuButton.onClick.RemoveListener(ExitToMainMenu);
            }

            courtroomController = null;
            caseSessionManager = null;
            activeCaseData = null;
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
                ResumeGame();
                return;
            }

            Show();
            courtroomController.Pause();
        }

        public void SaveGame()
        {
            if (caseSessionManager == null)
            {
                Debug.LogWarning("[PausePresenter] No active session manager bound for save.");
                return;
            }

            bool saved = caseSessionManager.SaveCurrentCase();
            if (saved)
            {
                Debug.Log("[PausePresenter] Game saved.");
            }
            else
            {
                Debug.LogWarning("[PausePresenter] Save failed: no active case session.");
            }
        }

        public void LoadGame()
        {
            if (caseSessionManager == null)
            {
                Debug.LogWarning("[PausePresenter] No active session manager bound for load.");
                return;
            }

            if (activeCaseData == null)
            {
                Debug.LogWarning("[PausePresenter] No active case data bound for load.");
                return;
            }

            GameSaveData saveData = caseSessionManager.LoadSavedGame();
            if (saveData == null)
            {
                Debug.LogWarning("[PausePresenter] No save file found to load.");
                return;
            }

            bool restored = caseSessionManager.RestoreFromSave(
                saveData,
                activeCaseData);

            if (restored)
            {
                Hide();
                if (courtroomController != null)
                {
                    courtroomController.Resume();
                }

                Debug.Log("[PausePresenter] Game restored from save.");
            }
            else
            {
                Debug.LogWarning("[PausePresenter] Failed to restore save data.");
            }
        }

        public void ResumeGame()
        {
            if (courtroomController == null)
            {
                return;
            }

            Hide();
            courtroomController.Resume();
        }

        public void ExitToMainMenu()
        {
            if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                Debug.Log($"[PausePresenter] Loading main menu scene '{mainMenuSceneName}'");
                SceneManager.LoadScene(mainMenuSceneName);
                return;
            }

            Debug.LogWarning("[PausePresenter] No main menu scene name configured.");
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
