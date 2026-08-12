using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Verdict.Data.Cases;
using Verdict.Systems;

namespace Verdict.UI.Overlay
{
    /// <summary>
    /// Presents the ending reached by the active courtroom session.
    ///
    /// Responsibilities:
    /// - Listen to CourtroomController.EndingTriggered.
    /// - Display EndingData.
    /// - Wait for the player to acknowledge the ending.
    /// - Tell the CourtroomController to end the case.
    ///
    /// Does NOT:
    /// - Determine which ending was reached.
    /// - Evaluate ending conditions.
    /// - Resolve arguments.
    /// - Modify court state directly.
    /// </summary>
    public sealed class EndingPresenter : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private GameObject backdrop;

        [Header("Canvas Groups")]
        [SerializeField]
        // Optional: assign a CanvasGroup for the panel. If left null the
        // presenter will try to get/add one at runtime and fall back to
        // SetActive on the GameObject for compatibility.
        private CanvasGroup panelGroup;

        [SerializeField]
        private CanvasGroup backdropGroup;

        [Header("Content")]
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text descriptionText;

        [Header("Controls")]
        [SerializeField]
        private Button continueButton;

        [SerializeField]
        [Tooltip("Optional: scene name to load when the player acknowledges the ending.")]
        private string mainMenuSceneName;

        private CourtroomController courtroomController;

        private bool isShowing;

        /// <summary>
        /// Fired after the player acknowledges the ending.
        /// Useful for another system such as a case result screen,
        /// main menu flow, or scene transition.
        /// </summary>
        public event Action EndingAcknowledged;

        public bool IsShowing =>
            isShowing;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(
                    HandleContinuePressed);
            }
            else
            {
                Debug.LogWarning(
                    "EndingPresenter.Awake: continueButton is not assigned.");
            }

            if (panel == null)
            {
                Debug.LogWarning(
                    "EndingPresenter.Awake: panel reference is not assigned.");
            }

            if (backdrop == null)
            {
                Debug.LogWarning(
                    "EndingPresenter.Awake: backdrop reference is not assigned.");
            }

            // If canvas groups weren't assigned in the inspector, try to
            // locate them on the provided GameObjects. Adding a CanvasGroup
            // is safe at runtime even if the object is inactive, but note
            // that the presenter must itself be enabled to run Awake/Start.
            if (panel != null && panelGroup == null)
            {
                panelGroup = panel.GetComponent<CanvasGroup>();
                if (panelGroup == null)
                {
                    panelGroup = panel.AddComponent<CanvasGroup>();
                }
            }

            if (backdrop != null && backdropGroup == null)
            {
                backdropGroup = backdrop.GetComponent<CanvasGroup>();
                if (backdropGroup == null)
                {
                    backdropGroup = backdrop.AddComponent<CanvasGroup>();
                }
            }

            // Start hidden by making alpha 0 / non-interactable. This avoids
            // toggling GameObject.active which breaks presenters attached
            // to those same GameObjects.
            Hide();
        }

        /// <summary>
        /// Binds this presenter to the active courtroom controller.
        /// </summary>
        public void Bind(
            CourtroomController controller)
        {
            Unbind();

            courtroomController = controller;

            if (courtroomController == null)
            {
                Debug.LogWarning(
                    "EndingPresenter.Bind called with a null controller.");
                return;
            }

            courtroomController.EndingTriggered +=
                HandleEndingTriggered;

            Debug.Log(
                "EndingPresenter bound to CourtroomController.");
        }

        /// <summary>
        /// Removes the event subscription.
        /// </summary>
        public void Unbind()
        {
            if (courtroomController == null)
            {
                return;
            }

            courtroomController.EndingTriggered -=
                HandleEndingTriggered;

            courtroomController = null;
        }

        /// <summary>
        /// Displays the supplied ending.
        /// </summary>
        private void HandleEndingTriggered(
            EndingData ending)
        {
            if (ending == null)
            {
                Debug.LogWarning(
                    "EndingPresenter received a null ending.");

                return;
            }

            Debug.Log(
                $"EndingPresenter received ending '{ending.Title}'. Showing panel.");

            Show(ending);
        }

        /// <summary>
        /// Displays ending data without performing any game logic.
        /// </summary>
        public void Show(
            EndingData ending)
        {
            if (ending == null)
            {
                return;
            }

            if (titleText != null)
            {
                titleText.text =
                    ending.Title ?? string.Empty;
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    ending.Description ?? string.Empty;
            }

            if (panel == null)
            {
                Debug.LogWarning(
                    "EndingPresenter.Show called but the panel reference is not assigned.");
            }

            isShowing = true;

            // Prefer CanvasGroup alpha toggles. If unavailable fall back to
            // GameObject.SetActive so older setups keep working.
            if (backdropGroup != null)
            {
                backdropGroup.alpha = 1f;
                backdropGroup.interactable = true;
                backdropGroup.blocksRaycasts = true;
            }
            else if (backdrop != null)
            {
                backdrop.SetActive(true);
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = 1f;
                panelGroup.interactable = true;
                panelGroup.blocksRaycasts = true;
            }
            else if (panel != null)
            {
                panel.SetActive(true);
            }

            if (continueButton != null)
            {
                continueButton.interactable = true;

                // Optional:
                // automatically put focus on the button
                // when using keyboard/controller navigation.
                continueButton.Select();
            }
        }

        /// <summary>
        /// Hides the ending panel.
        /// </summary>
        public void Hide()
        {
            isShowing = false;

            // Mirror the Show() logic: prefer CanvasGroup transitions and
            // otherwise fall back to SetActive(false).
            if (backdropGroup != null)
            {
                backdropGroup.alpha = 0f;
                backdropGroup.interactable = false;
                backdropGroup.blocksRaycasts = false;
            }
            else if (backdrop != null)
            {
                backdrop.SetActive(false);
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = 0f;
                panelGroup.interactable = false;
                panelGroup.blocksRaycasts = false;
            }
            else if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void HandleContinuePressed()
        {
            if (!isShowing)
            {
                return;
            }

            Debug.Log("EndingPresenter: continue button pressed.");

            // Prevent double-clicks from triggering EndCase twice.
            isShowing = false;

            if (continueButton != null)
            {
                continueButton.interactable = false;
            }

            Hide();

            EndingAcknowledged?.Invoke();

            if (courtroomController != null)
            {
                courtroomController.EndCase();
            }
            else
            {
                Debug.LogWarning(
                    "EndingPresenter: no courtroomController assigned when continue was pressed.");
            }

            if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                Debug.Log($"EndingPresenter: loading main menu scene '{mainMenuSceneName}'");
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        private void OnDestroy()
        {
            Unbind();

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(
                    HandleContinuePressed);
            }
        }
    }
}
