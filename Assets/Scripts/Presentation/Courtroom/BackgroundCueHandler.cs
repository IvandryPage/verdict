using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Narrative;

namespace Verdict.Presentation.Courtroom
{
    /// <summary>
    /// Example IPresentationEventHandler for background changes. Uses
    /// two stacked Image components and crossfades between them so the
    /// change isn't a jarring pop.
    /// </summary>
    public sealed class BackgroundCueHandler : MonoBehaviour, IPresentationEventHandler
    {
        [SerializeField] private BackgroundLibrary library;
        [SerializeField] private Image currentImage;
        [SerializeField] private Image incomingImage;
        [SerializeField] private float defaultCrossfadeDuration = 0.35f;

        private Coroutine activeRoutine;

        public bool CanHandle(NarrativeEventType type)
        {
            return type == NarrativeEventType.ChangeBackground;
        }

        public void Handle(NarrativeEventData eventData)
        {
            if (library == null || currentImage == null || incomingImage == null)
            {
                return;
            }

            if (!library.TryGetBackground(eventData.Parameter, out Sprite background))
            {
                Debug.LogWarning($"{nameof(BackgroundCueHandler)}: No background registered for id '{eventData.Parameter}'.");
                return;
            }

            float duration = eventData.Value > 0f ? eventData.Value : defaultCrossfadeDuration;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(CrossfadeRoutine(background, duration));
        }

        private IEnumerator CrossfadeRoutine(Sprite background, float duration)
        {
            incomingImage.sprite = background;

            Color incomingColor = incomingImage.color;
            incomingColor.a = 0f;
            incomingImage.color = incomingColor;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                incomingColor.a = Mathf.Clamp01(elapsed / duration);
                incomingImage.color = incomingColor;

                yield return null;
            }

            incomingColor.a = 1f;
            incomingImage.color = incomingColor;

            currentImage.sprite = background;

            Color settledColor = currentImage.color;
            settledColor.a = 1f;
            currentImage.color = settledColor;

            incomingColor.a = 0f;
            incomingImage.color = incomingColor;

            activeRoutine = null;
        }
    }
}
