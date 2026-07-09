using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Evidence;

namespace Verdict.UI
{
    public sealed class EvidenceItemView : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private Image background;

        public event Action<EvidenceData> Selected;

        public EvidenceData Evidence { get; private set; }

        public void Bind(EvidenceData evidence)
        {
            Evidence = evidence;

            titleText.text = evidence.DisplayName;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            Selected?.Invoke(Evidence);
        }

        public void SetSelected(bool selected)
        {
            background.color =
                selected
                ? Color.yellow
                : Color.black;
        }
    }
}
