using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Verdict.Systems.Save;

namespace Verdict.UI.MainMenu
{
    public sealed class MainMenuPresenter : MonoBehaviour
    {
        public static bool ShouldLoadSavedGame { get; set; }

        [Header("Buttons")]
        [SerializeField]
        private Button newGameButton;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private Button exitButton;

        [Header("Loading UI")]
        [SerializeField]
        private GameObject loadingPanel;

        [SerializeField]
        private TMP_Text loadingText;

        [Header("Scenes")]
        [SerializeField]
        private string newGameSceneName = "Courtroom";

        [SerializeField]
        private string continueSceneName = "Courtroom";

        [SerializeField]
        private string saveFileName = "savegame.json";

        private bool isLoading;

        private void Awake()
        {
            EnsureLoadingOverlay();
            RefreshContinueButton();
            SetLoadingVisible(false);

            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(StartNewGame);
                newGameButton.onClick.AddListener(StartNewGame);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ContinueOrLoadGame);
                continueButton.onClick.AddListener(ContinueOrLoadGame);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(QuitGame);
                exitButton.onClick.AddListener(QuitGame);
            }
        }

        public void StartNewGame()
        {
            if (isLoading)
            {
                return;
            }

            ShouldLoadSavedGame = false;

            if (string.IsNullOrWhiteSpace(newGameSceneName))
            {
                Debug.LogWarning("[MainMenuPresenter] New game scene name is empty.");
                return;
            }

            StartCoroutine(LoadSceneWithLoading(newGameSceneName));
        }

        public void ContinueOrLoadGame()
        {
            if (isLoading)
            {
                return;
            }

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

            ShouldLoadSavedGame = true;

            if (string.IsNullOrWhiteSpace(continueSceneName))
            {
                Debug.LogWarning("[MainMenuPresenter] Continue scene name is empty.");
                ShouldLoadSavedGame = false;
                return;
            }

            StartCoroutine(LoadSceneWithLoading(continueSceneName, true));
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

        private IEnumerator LoadSceneWithLoading(string sceneName, bool isContinueLoad = false)
        {
            isLoading = true;
            SetLoadingVisible(true);

            if (loadingText != null)
            {
                loadingText.text = isContinueLoad
                    ? "Memuat save game..."
                    : "Memuat kasus...";
            }

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = true;

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            isLoading = false;
            SetLoadingVisible(false);
        }

        private void RefreshContinueButton()
        {
            if (continueButton == null)
            {
                return;
            }

            string savePath = Path.Combine(
                Application.persistentDataPath,
                "VerdictSaves",
                saveFileName);

            bool hasSave = File.Exists(savePath);
            continueButton.gameObject.SetActive(hasSave);
            continueButton.interactable = hasSave;
        }

        private void EnsureLoadingOverlay()
        {
            if (loadingPanel != null)
            {
                if (loadingText == null)
                {
                    loadingText = loadingPanel.GetComponentInChildren<TMP_Text>();
                }

                return;
            }

            GameObject root = new GameObject("MainMenuLoadingOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            GameObject panel = new GameObject("LoadingPanel", typeof(Image));
            panel.transform.SetParent(root.transform, false);

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.7f);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            GameObject textObject = new GameObject("LoadingText", typeof(TextMeshProUGUI));
            textObject.transform.SetParent(panel.transform, false);

            loadingText = textObject.GetComponent<TMP_Text>();
            loadingText.text = "Memuat...";
            loadingText.alignment = TextAlignmentOptions.Center;
            loadingText.color = Color.white;

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            loadingPanel = root;
            loadingPanel.SetActive(false);
        }

        private void SetLoadingVisible(bool visible)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(visible);
            }
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
