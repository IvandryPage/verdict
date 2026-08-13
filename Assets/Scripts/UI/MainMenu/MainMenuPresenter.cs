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

        private Image spinnerImage;
        private Coroutine spinnerCoroutine;

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

            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;

            TMP_Text buttonText = continueButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = hasSave ? "LANJUTKAN" : "MULAI";
            }
        }

        private void EnsureLoadingOverlay()
        {
            if (loadingPanel != null)
            {
                if (loadingText == null)
                {
                    loadingText = loadingPanel.GetComponentInChildren<TMP_Text>();
                }

                spinnerImage = loadingPanel.transform.Find("Spinner")?.GetComponent<Image>();
                return;
            }

            GameObject root = new GameObject(
                "MainMenuLoadingBadge",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(1f, 0f);
            rootRect.anchorMax = new Vector2(1f, 0f);
            rootRect.pivot = new Vector2(1f, 0f);
            rootRect.sizeDelta = new Vector2(220f, 56f);
            rootRect.anchoredPosition = new Vector2(-20f, 20f);

            GameObject panel = new GameObject("LoadingPanel", typeof(Image));
            panel.transform.SetParent(root.transform, false);

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.09f, 0.11f, 0.92f);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            GameObject spinnerObject = new GameObject("Spinner", typeof(Image));
            spinnerObject.transform.SetParent(panel.transform, false);

            spinnerImage = spinnerObject.GetComponent<Image>();
            spinnerImage.type = Image.Type.Filled;
            spinnerImage.fillMethod = Image.FillMethod.Radial360;
            spinnerImage.fillAmount = 0.75f;
            spinnerImage.color = new Color(0.72f, 0.9f, 1f, 1f);

            RectTransform spinnerRect = spinnerObject.GetComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0f, 0.5f);
            spinnerRect.pivot = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(18f, 18f);
            spinnerRect.anchoredPosition = new Vector2(16f, 0f);

            GameObject textObject = new GameObject("LoadingText", typeof(TextMeshProUGUI));
            textObject.transform.SetParent(panel.transform, false);

            loadingText = textObject.GetComponent<TMP_Text>();
            loadingText.text = "Memuat...";
            loadingText.alignment = TextAlignmentOptions.Left;
            loadingText.fontSize = 13;
            loadingText.color = Color.white;

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(1f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(-36f, 22f);
            textRect.anchoredPosition = new Vector2(18f, 0f);

            loadingPanel = root;
            loadingPanel.SetActive(false);
        }

        private void SetLoadingVisible(bool visible)
        {
            if (loadingPanel == null)
            {
                return;
            }

            loadingPanel.SetActive(visible);

            if (visible)
            {
                if (spinnerCoroutine == null)
                {
                    spinnerCoroutine = StartCoroutine(AnimateSpinner());
                }
            }
            else if (spinnerCoroutine != null)
            {
                StopCoroutine(spinnerCoroutine);
                spinnerCoroutine = null;

                if (spinnerImage != null)
                {
                    spinnerImage.rectTransform.localRotation = Quaternion.identity;
                }
            }
        }

        private IEnumerator AnimateSpinner()
        {
            while (true)
            {
                if (spinnerImage != null)
                {
                    spinnerImage.rectTransform.Rotate(0f, 0f, -220f * Time.deltaTime);
                }

                yield return null;
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
