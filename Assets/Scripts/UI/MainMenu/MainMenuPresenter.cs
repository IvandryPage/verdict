using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Verdict.Systems.Save;

namespace Verdict.UI.MainMenu
{
    public sealed class MainMenuPresenter : MonoBehaviour
    {
        public static bool ShouldLoadSavedGame { get; set; }

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button exitButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Image spinner;
        [SerializeField] private TMP_Text loadingText;

        [Header("Scenes")]
        [SerializeField] private string newGameSceneName = "Courtroom";
        [SerializeField] private string continueSceneName = "Courtroom";

        [Header("Save")]
        [SerializeField] private string saveFileName = "savegame.json";

        private bool isLoading;

        private void Awake()
        {
            RefreshContinueButton();

            newGameButton?.onClick.AddListener(StartNewGame);
            continueButton?.onClick.AddListener(ContinueGame);
            exitButton?.onClick.AddListener(QuitGame);

            SetLoading(false);
        }

        private void OnDestroy()
        {
            newGameButton?.onClick.RemoveListener(StartNewGame);
            continueButton?.onClick.RemoveListener(ContinueGame);
            exitButton?.onClick.RemoveListener(QuitGame);
        }

        public void StartNewGame()
        {
            if (isLoading)
                return;

            ShouldLoadSavedGame = false;

            if (string.IsNullOrWhiteSpace(newGameSceneName))
                return;

            StartCoroutine(LoadScene(newGameSceneName, "Memuat kasus..."));
        }

        public void ContinueGame()
        {
            if (isLoading)
                return;

            string savePath = Path.Combine(
                Application.persistentDataPath,
                "VerdictSaves",
                saveFileName);

            if (!File.Exists(savePath))
            {
                StartNewGame();
                return;
            }

            ShouldLoadSavedGame = true;

            if (string.IsNullOrWhiteSpace(continueSceneName))
                return;

            StartCoroutine(
                LoadScene(
                    continueSceneName,
                    "Memuat persidangan..."));
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadScene(
            string sceneName,
            string message)
        {
            isLoading = true;
            SetLoading(true);

            if (loadingText != null)
                loadingText.text = message;

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                yield return null;
            }
        }

        private void RefreshContinueButton()
        {
            if (continueButton == null)
                return;

            string savePath = Path.Combine(
                Application.persistentDataPath,
                "VerdictSaves",
                saveFileName);

            bool hasSave = File.Exists(savePath);

            if (!hasSave)
            {
                continueButton.gameObject.SetActive(false);
            }
        }

        private void SetLoading(bool visible)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(visible);

            if (spinner != null)
                spinner.gameObject.SetActive(visible);
        }

        private void Update()
        {
            if (!isLoading || spinner == null)
                return;

            spinner.transform.Rotate(
                0f,
                0f,
                -180f * Time.unscaledDeltaTime);
        }
    }
}
