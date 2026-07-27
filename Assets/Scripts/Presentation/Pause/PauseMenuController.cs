using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Verdict.Input;
using Verdict.Presentation.Evidence;
using Verdict.Presentation.Settings;

namespace Verdict.Presentation.Pause
{
    /// <summary>
    /// Escape/Start toggles this. Time.timeScale = 0 while open, which
    /// naturally freezes the typewriter, auto-advance timers and camera
    /// cues too since they all use Time.deltaTime / WaitForSeconds - no
    /// extra bookkeeping needed elsewhere for "pause everything".
    /// </summary>
    public sealed class PauseMenuController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button checkEvidenceButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Panels")]
        [SerializeField] private EvidencePanel evidencePanel;
        [SerializeField] private SettingsView settingsView;

        [Header("Scene")]
        [SerializeField] private string mainMenuScene = "01_MainMenu";

        private VerdictInputActions inputActions;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            inputActions = new VerdictInputActions();
            SetVisible(false);
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();
            inputActions.Player.Pause.performed += HandlePausePerformed;

            resumeButton?.onClick.AddListener(Resume);
            checkEvidenceButton?.onClick.AddListener(HandleCheckEvidenceClicked);
            settingsButton?.onClick.AddListener(HandleSettingsClicked);
            mainMenuButton?.onClick.AddListener(HandleMainMenuClicked);
        }

        private void OnDisable()
        {
            inputActions.Player.Pause.performed -= HandlePausePerformed;
            inputActions.Player.Disable();

            resumeButton?.onClick.RemoveListener(Resume);
            checkEvidenceButton?.onClick.RemoveListener(HandleCheckEvidenceClicked);
            settingsButton?.onClick.RemoveListener(HandleSettingsClicked);
            mainMenuButton?.onClick.RemoveListener(HandleMainMenuClicked);
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }

        private void HandlePausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (IsPaused)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            SetVisible(true);
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            SetVisible(false);
        }

        private void HandleCheckEvidenceClicked()
        {
            // Browse-only here - presenting evidence only makes sense
            // while a statement is actually active, and the pause menu
            // by definition isn't that moment.
            evidencePanel?.Open(allowPresenting: false);
        }

        private void HandleSettingsClicked()
        {
            settingsView?.Open();
        }

        private void HandleMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }

        private void SetVisible(bool visible)
        {
            root?.SetActive(visible);
        }
    }
}
