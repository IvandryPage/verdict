using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Verdict.Data.Cases;
using Verdict.Presentation.HUD;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Shown briefly when the case finishes - which ending was reached
    /// plus final stats - then returns to the main menu. No restart here
    /// on purpose: replaying means going through the main menu again,
    /// same as starting any other case.
    /// </summary>
    public sealed class VerdictResultView : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("Root")]
        [SerializeField] private CanvasGroup root;

        [Header("Ending")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Final Stats (optional)")]
        [SerializeField] private List<StatGaugeView> finalStatGauges = new();

        [Header("Navigation")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string mainMenuScene = "01_MainMenu";

        [Tooltip("If set, automatically returns to the main menu after this many seconds even if the player doesn't click the button. 0 disables auto-return.")]
        [SerializeField] private float autoReturnDelay = 0f;

        private CourtroomController controller;
        private EndingData lastEnding;
        private Coroutine autoReturnRoutine;

        private void Awake()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<CourtroomBootstrap>();
            }

            SetVisible(false);
        }

        private void Start()
        {
            if (bootstrap == null || bootstrap.Controller == null)
            {
                Debug.LogError($"{nameof(VerdictResultView)}: Missing CourtroomBootstrap or Controller.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;
            controller.EndingTriggered += HandleEndingTriggered;
            controller.CaseFinished += HandleCaseFinished;
        }

        private void OnEnable()
        {
            mainMenuButton?.onClick.AddListener(ReturnToMainMenu);
        }

        private void OnDisable()
        {
            mainMenuButton?.onClick.RemoveListener(ReturnToMainMenu);
        }

        private void OnDestroy()
        {
            if (controller == null)
            {
                return;
            }

            controller.EndingTriggered -= HandleEndingTriggered;
            controller.CaseFinished -= HandleCaseFinished;
        }

        private void HandleEndingTriggered(EndingData ending)
        {
            lastEnding = ending;
        }

        private void HandleCaseFinished()
        {
            if (titleText != null)
            {
                titleText.text = lastEnding != null ? lastEnding.Title : "Case Closed";
            }

            if (descriptionText != null)
            {
                descriptionText.text = lastEnding != null ? lastEnding.Description : string.Empty;
            }

            CourtStateRuntime state = controller?.CourtState;

            if (state != null)
            {
                foreach (StatGaugeView gauge in finalStatGauges)
                {
                    if (gauge != null)
                    {
                        gauge.SetValue(state.GetCourtStat(gauge.Stat));
                    }
                }
            }

            SetVisible(true);

            if (autoReturnDelay > 0f)
            {
                autoReturnRoutine = StartCoroutine(AutoReturnRoutine());
            }
        }

        private IEnumerator AutoReturnRoutine()
        {
            yield return new WaitForSecondsRealtime(autoReturnDelay);
            ReturnToMainMenu();
        }

        private void ReturnToMainMenu()
        {
            if (autoReturnRoutine != null)
            {
                StopCoroutine(autoReturnRoutine);
                autoReturnRoutine = null;
            }

            SceneManager.LoadScene(mainMenuScene);
        }

        private void SetVisible(bool visible)
        {
            if (root == null)
            {
                return;
            }

            root.alpha = visible ? 1f : 0f;
            root.interactable = visible;
            root.blocksRaycasts = visible;
        }
    }
}
