using System.Collections;
using UnityEngine;
using Verdict.Data.Narrative;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Example IPresentationEventHandler for screen fades. Parameter
    /// picks the fade color by name ("black", "white", or a hex string
    /// like "#221133"), Value is duration in seconds. Fades to opaque
    /// then automatically back to clear - a single ScreenFade event is a
    /// full "fade out, beat, fade in", not just one direction.
    /// </summary>
    public sealed class ScreenFadeHandler : MonoBehaviour, IPresentationEventHandler
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private UnityEngine.UI.Image fadeImage;
        [SerializeField] private float defaultDuration = 0.4f;
        [SerializeField] private float holdWhileOpaque = 0.15f;

        private Coroutine activeRoutine;

        public bool CanHandle(NarrativeEventType type)
        {
            return type == NarrativeEventType.ScreenFade;
        }

        public void Handle(NarrativeEventData eventData)
        {
            if (fadeGroup == null)
            {
                return;
            }

            if (fadeImage != null && ColorUtility.TryParseHtmlString(
                    NormalizeColorName(eventData.Parameter), out Color color))
            {
                color.a = 1f;
                fadeImage.color = color;
            }

            float duration = eventData.Value > 0f ? eventData.Value : defaultDuration;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(FadeRoutine(duration));
        }

        private static string NormalizeColorName(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return "#000000";
            }

            return parameter.StartsWith("#") ? parameter : $"#{ColorNameToHex(parameter)}";
        }

        private static string ColorNameToHex(string name)
        {
            return name.ToLowerInvariant() switch
            {
                "white" => "FFFFFF",
                "red" => "AA2222",
                _ => "000000"
            };
        }

        private IEnumerator FadeRoutine(float duration)
        {
            yield return Fade(0f, 1f, duration);
            yield return new WaitForSeconds(holdWhileOpaque);
            yield return Fade(1f, 0f, duration);

            activeRoutine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            fadeGroup.alpha = to;
        }
    }
}
