using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Evidence;
using Verdict.Systems;

namespace Verdict.UI.Evidence
{
    public sealed class EvidenceUnlockedPresenter : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private CanvasGroup panelGroup;

        [Header("Content")]

        [SerializeField]
        private TMP_Text messageText;

        [SerializeField]
        private TMP_Text evidenceNameText;

        [SerializeField]
        private TMP_Text evidenceDescriptionText;

        [SerializeField]
        private Image evidenceIcon;

        [Header("Buttons")]
        [SerializeField]
        private Button closeButton;

        private CourtroomController courtroomController;

        private void Awake()
        {
            if (panel != null && panelGroup == null)
            {
                panelGroup = panel.GetComponent<CanvasGroup>();

                if (panelGroup == null)
                {
                    panelGroup = panel.AddComponent<CanvasGroup>();
                }
            }

            Hide();
        }

        public void Bind(CourtroomController controller)
        {
            Unbind();

            courtroomController = controller;

            if (courtroomController != null)
            {
                courtroomController.EvidenceUnlocked += HandleEvidenceUnlocked;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Unbind()
        {
            if (courtroomController != null)
            {
                courtroomController.EvidenceUnlocked -= HandleEvidenceUnlocked;
                courtroomController = null;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }
        }

        private void HandleEvidenceUnlocked(EvidenceData evidence)
        {
            Show(evidence);
        }

        public void Show(EvidenceData evidence)
        {
            if (evidence == null)
            {
                return;
            }

            if (messageText != null)
            {
                messageText.text = "Bukti Terbuka";
            }

            if (evidenceNameText != null)
            {
                evidenceNameText.text = evidence.DisplayName ?? string.Empty;
            }

            if (evidenceDescriptionText != null)
            {
                evidenceDescriptionText.text = evidence.Description ?? string.Empty;
            }

            if (evidenceIcon != null)
            {
                evidenceIcon.sprite = evidence.Icon;
                evidenceIcon.enabled = evidence.Icon != null;
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
        }

        public void Hide()
        {
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

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
