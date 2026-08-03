using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Verdict.Presentation.Settings;

namespace Verdict.Presentation.MainMenu
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string courtroomScene = "02_Courtroom";

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Settings")]
        [SerializeField] private SettingsView settingsView;

        private void OnEnable()
        {
            playButton?.onClick.AddListener(HandlePlayClicked);
            settingsButton?.onClick.AddListener(HandleSettingsClicked);
            quitButton?.onClick.AddListener(HandleQuitClicked);
        }

        private void OnDisable()
        {
            playButton?.onClick.RemoveListener(HandlePlayClicked);
            settingsButton?.onClick.RemoveListener(HandleSettingsClicked);
            quitButton?.onClick.RemoveListener(HandleQuitClicked);
        }

        private void HandlePlayClicked()
        {
            SceneManager.LoadScene(courtroomScene);
        }

        private void HandleSettingsClicked()
        {
            settingsView?.Open();
        }

        private void HandleQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
