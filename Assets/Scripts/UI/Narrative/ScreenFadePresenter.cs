using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Verdict.UI.Narrative
{
    public sealed class ScreenFadePresenter : MonoBehaviour
    {
        [SerializeField]
        private Image fadeImage;

        [SerializeField]
        private Color defaultColor = new Color(0f, 0f, 0f, 1f);

        [SerializeField]
        private float defaultDuration = 0.35f;

        [SerializeField]
        private float defaultHoldDuration = 0.15f;

        private Coroutine fadeRoutine;

        private void Awake()
        {
            if (fadeImage == null)
            {
                var canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    var go = new GameObject("ScreenFadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    go.transform.SetParent(canvas.transform, false);
                    go.transform.SetAsLastSibling();
                    var rect = go.GetComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    fadeImage = go.GetComponent<Image>();
                }
            }

            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
                fadeImage.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0f);
            }
        }

        public void Trigger(string colorName, float duration)
        {
            Trigger(colorName, duration, defaultHoldDuration);
        }

        public void Trigger(string colorName, float duration, float holdDuration)
        {
            if (fadeImage == null)
            {
                return;
            }

            Color targetColor = ResolveColor(colorName);
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            float safeDuration = Mathf.Max(0.15f, duration > 0f ? duration : defaultDuration);
            float safeHold = Mathf.Max(0.05f, holdDuration);
            fadeRoutine = StartCoroutine(FadeRoutine(targetColor, safeDuration, safeHold));
        }

        private IEnumerator FadeRoutine(Color targetColor, float duration, float holdDuration)
        {
            float fadeInDuration = Mathf.Min(duration, 0.9f);
            float fadeOutDuration = Mathf.Max(0.45f, duration);
            float hold = Mathf.Max(0.1f, holdDuration);

            Color startColor = fadeImage.color;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                fadeImage.color = Color.Lerp(startColor, targetColor, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            fadeImage.color = targetColor;
            yield return new WaitForSeconds(hold);

            elapsed = 0f;
            Color fadeOutColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);
                fadeImage.color = Color.Lerp(targetColor, fadeOutColor, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            fadeImage.color = fadeOutColor;
            fadeRoutine = null;
        }

        private Color ResolveColor(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return defaultColor;
            }

            string normalized = parameter.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "black":
                    return Color.black;
                case "white":
                    return Color.white;
                case "red":
                    return Color.red;
                case "blue":
                    return Color.blue;
                case "green":
                    return Color.green;
                case "yellow":
                    return Color.yellow;
                default:
                    return defaultColor;
            }
        }
    }
}
