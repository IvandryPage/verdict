using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Evidence;

namespace Verdict.UI.Evidence
{
    public sealed class EvidenceDetailView : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField]
        private Image artworkImage;

        [SerializeField]
        private TMP_Text evidenceNameText;

        [SerializeField]
        private TMP_Text evidenceTypeText;

        [SerializeField]
        private TMP_Text descriptionText;

        [Header("Actions")]
        [SerializeField]
        private Button selectButton;

        [SerializeField]
        private TMP_Text selectButtonText;

        private EvidenceData evidence;
        private Action<EvidenceData> selectCallback;

        public void Show(
            EvidenceData evidence,
            bool isSelected,
            Action<EvidenceData> onSelect,
            Action onClose)
        {
            this.evidence = evidence;
            selectCallback = onSelect;

            if (evidence == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);

            if (artworkImage != null)
            {
                artworkImage.sprite =
                    evidence.Artwork;

                artworkImage.enabled =
                    evidence.Artwork != null;
            }

            if (evidenceNameText != null)
            {
                evidenceNameText.text =
                    evidence.DisplayName ?? string.Empty;
            }

            if (evidenceTypeText != null)
            {
                evidenceTypeText.text =
                    evidence.Type.ToString();
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    evidence.Description ?? string.Empty;
            }

            UpdateSelectionState(isSelected);

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(
                    HandleSelectClicked);
            }
        }

        public void UpdateSelectionState(
            bool isSelected)
        {
            if (selectButtonText != null)
            {
                selectButtonText.text =
                    isSelected
                        ? "Deselect"
                        : "Select";
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            evidence = null;
            selectCallback = null;
        }

        private void HandleSelectClicked()
        {
            if (evidence == null)
            {
                return;
            }

            selectCallback?.Invoke(evidence);
        }


        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
            }
        }
    }
}
