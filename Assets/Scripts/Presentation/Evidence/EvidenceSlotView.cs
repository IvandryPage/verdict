using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Evidence;

namespace Verdict.Presentation.Evidence
{
    /// <summary>
    /// One evidence item in the panel. Put this on a prefab with a
    /// Button, an Image for the icon, and a TMP_Text for the name.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class EvidenceSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;

        private Button button;
        private EvidenceData data;

        public EvidenceData Data => data;

        public event Action<EvidenceData> Clicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        public void Bind(EvidenceData evidence)
        {
            data = evidence;

            if (nameText != null)
            {
                nameText.text = evidence.DisplayName;
            }

            if (iconImage != null)
            {
                iconImage.sprite = evidence.Icon;
                iconImage.enabled = evidence.Icon != null;
            }
        }

        private void HandleClick()
        {
            Clicked?.Invoke(data);
        }
    }
}
