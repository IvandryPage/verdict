using System;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Evidence;

namespace Verdict.UI.Evidence
{
    public sealed class EvidenceOptionView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private Button button;

        [SerializeField]
        private Image iconImage;

        [Header("Selection")]
        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color selectedColor =
            new Color(0.75f, 0.85f, 1f, 1f);

        private EvidenceData evidence;
        private Action<EvidenceData> clickCallback;

        public EvidenceData Evidence => evidence;

        public void Setup(
            EvidenceData evidence,
            Action<EvidenceData> onClicked)
        {
            this.evidence = evidence;
            clickCallback = onClicked;

            if (iconImage != null)
            {
                iconImage.sprite =
                    evidence?.Icon;

                iconImage.enabled =
                    evidence?.Icon != null;
            }

            SetSelected(false);

            if (button == null)
            {
                Debug.LogError(
                    "EvidenceOptionView: Button is not assigned.",
                    this);

                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(
                HandleClicked);
        }

        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color =
                    selected
                        ? selectedColor
                        : normalColor;
            }
        }

        private void HandleClicked()
        {
            if (evidence == null)
            {
                return;
            }

            clickCallback?.Invoke(evidence);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(
                    HandleClicked);
            }
        }
    }
}
