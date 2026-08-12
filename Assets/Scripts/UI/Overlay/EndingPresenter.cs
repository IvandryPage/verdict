using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("Content")]
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text descriptionText;

        [Header("Controls")]
        [SerializeField]
        private Button continueButton;

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
                return;
            }

            courtroomController.EndingTriggered +=
                HandleEndingTriggered;
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

            isShowing = true;

            if (panel != null)
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

            if (panel != null)
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
