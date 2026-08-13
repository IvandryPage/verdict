using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Verdict.UI.Narrative
{
    public sealed class ChapterPresenter : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private CanvasGroup panelGroup;

        [SerializeField]
        private TMP_Text chapterTitleText;

        [SerializeField]
        private TMP_Text chapterSubtitleText;

        [Header("Timing")]
        [SerializeField]
        private float displayDuration = 2f;

        [SerializeField]
        private float fadeDuration = 0.25f;

        private Coroutine hideRoutine;

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

            HideImmediate();
        }

        public void ShowChapter(string chapterTitle, string subtitle = null)
        {
            Debug.Log("ShowChapter: " + chapterTitle);
            if (string.IsNullOrWhiteSpace(chapterTitle))
            {
                return;
            }

            if (panel == null)
            {
                return;
            }

            panel.SetActive(true);

            if (chapterTitleText != null)
            {
                chapterTitleText.text = chapterTitle.Trim();
            }

            if (chapterSubtitleText != null)
            {
                chapterSubtitleText.text = string.IsNullOrWhiteSpace(subtitle)
                    ? string.Empty
                    : subtitle.Trim();
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = 0f;
                panelGroup.interactable = true;
                panelGroup.blocksRaycasts = true;
                if (isActiveAndEnabled)
                {
                    StartCoroutine(FadeInRoutine());
                }
            }

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }

            if (isActiveAndEnabled)
            {
                hideRoutine = StartCoroutine(HideAfterDelay());
            }
        }

        private IEnumerator FadeInRoutine()
        {
            if (panelGroup == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                panelGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            panelGroup.alpha = 1f;
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);

            if (panelGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    panelGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                    yield return null;
                }

                panelGroup.alpha = 0f;
                panelGroup.interactable = false;
                panelGroup.blocksRaycasts = false;
            }

            if (panel != null)
            {
                panel.SetActive(false);
            }

            hideRoutine = null;
        }

        public void HideImmediate()
        {
            if (panelGroup != null)
            {
                panelGroup.alpha = 0f;
                panelGroup.interactable = false;
                panelGroup.blocksRaycasts = false;
            }

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
