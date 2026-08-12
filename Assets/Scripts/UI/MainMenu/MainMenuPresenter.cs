using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Verdict.Systems.Save;

namespace Verdict.UI.MainMenu
{
    public sealed class MainMenuPresenter : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField]
        private Button newGameButton;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private Button exitButton;

        [Header("Scenes")]
        [SerializeField]
        private string newGameSceneName = "Courtroom";

        [SerializeField]
        private string continueSceneName = "Courtroom";

        [SerializeField]
        private string saveFileName = "savegame.json";

        private void Awake()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(StartNewGame);
                newGameButton.onClick.AddListener(StartNewGame);
            }

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
                continueButton.onClick.RemoveListener(ContinueOrLoadGame);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(QuitGame);
                exitButton.onClick.AddListener(QuitGame);
            }
        }

        public void StartNewGame()
        {
            if (string.IsNullOrWhiteSpace(newGameSceneName))
            {
                Debug.LogWarning("[MainMenuPresenter] New game scene name is empty.");
                return;
            }

            Debug.Log($"[MainMenuPresenter] Loading new game scene '{newGameSceneName}'");
            SceneManager.LoadScene(newGameSceneName);
        }

        public void ContinueOrLoadGame()
        {
            string savePath = Path.Combine(
                Application.persistentDataPath,
                "VerdictSaves",
                saveFileName);

            if (!File.Exists(savePath))
            {
                Debug.Log("[MainMenuPresenter] No save file found. Starting a new game instead.");
                StartNewGame();
                return;
            }

            if (string.IsNullOrWhiteSpace(continueSceneName))
            {
                Debug.LogWarning("[MainMenuPresenter] Continue scene name is empty.");
                return;
            }

            Debug.Log($"[MainMenuPresenter] Loading saved game scene '{continueSceneName}'");
            SceneManager.LoadScene(continueSceneName);
        }

        public void QuitGame()
        {
            Debug.Log("[MainMenuPresenter] Quit requested.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(StartNewGame);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ContinueOrLoadGame);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(QuitGame);
            }
        }
    }
}
