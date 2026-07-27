using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Cases;

namespace Verdict.Presentation.HUD
{
    /// <summary>
    /// One stat bar (Judge Trust, Penalty, etc). Displayed on a 0-100
    /// scale by convention - CourtState doesn't define a hard max, so
    /// values above 100 just fill past the bar rather than erroring.
    /// </summary>
    public sealed class StatGaugeView : MonoBehaviour
    {
        [Header("Stat")]
        [SerializeField] private CourtStat stat;
        [SerializeField] private string label = "Stat";

        public CourtStat Stat => stat;

        [Header("UI")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        [Header("Warning")]
        [Tooltip("Fill color when the value is at/above this out of 100.")]
        [SerializeField] private int highThreshold = 60;
        [Tooltip("Fill color when the value is at/below this out of 100.")]
        [SerializeField] private int lowThreshold = 25;
        [SerializeField] private Color highColor = new(0.3f, 0.75f, 0.35f);
        [SerializeField] private Color midColor = new(0.85f, 0.7f, 0.2f);
        [SerializeField] private Color lowColor = new(0.8f, 0.25f, 0.25f);

        /// <summary>
        /// True while the stat is at/below lowThreshold - a HUD
        /// controller can use this to trigger a pulse/shake for tension.
        /// </summary>
        public bool IsInWarningState { get; private set; }

        private void Awake()
        {
            if (labelText != null)
            {
                labelText.text = label;
            }
        }

        public void SetValue(int value)
        {
            int clamped = Mathf.Clamp(value, 0, 100);

            if (fillImage != null)
            {
                fillImage.fillAmount = clamped / 100f;
                fillImage.color = GetColor(clamped);
            }

            if (valueText != null)
            {
                valueText.text = value.ToString();
            }

            IsInWarningState = clamped <= lowThreshold;
        }

        private Color GetColor(int value)
        {
            if (value <= lowThreshold)
            {
                return lowColor;
            }

            return value >= highThreshold ? highColor : midColor;
        }
    }
}
