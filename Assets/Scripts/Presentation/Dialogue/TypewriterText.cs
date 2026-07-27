using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Verdict.Presentation.Settings;

namespace Verdict.Presentation.Dialogue
{
    /// <summary>
    /// Reveals text one character at a time on a TMP_Text. Call Play()
    /// with new text, SkipToEnd() to instantly reveal the rest (the
    /// classic "tap to skip typing" behaviour), and listen to Completed
    /// for when the reveal finishes (naturally or via skip). Speed comes
    /// from GameSettings.DialogueSpeed, read fresh each time Play() is
    /// called, so the settings menu takes effect on the very next line.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TypewriterText : MonoBehaviour
    {
        [Header("Speed")]
        [Tooltip("If on, uses GameSettings.DialogueSpeed instead of the fixed value below.")]
        [SerializeField] private bool useGameSettings = true;

        [Tooltip("Characters revealed per second. Only used when Use Game Settings is off.")]
        [SerializeField] private float charactersPerSecond = 45f;

        [Tooltip("Extra pause (in characters-worth of time) after punctuation like . , ! ?")]
        [SerializeField] private float punctuationPauseMultiplier = 6f;

        private TMP_Text label;
        private Coroutine playRoutine;
        private string fullText = string.Empty;

        public bool IsPlaying { get; private set; }

        public event Action Completed;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
        }

        /// <summary>
        /// Starts revealing text from scratch. Any in-progress reveal is
        /// stopped first.
        /// </summary>
        public void Play(string text)
        {
            fullText = text ?? string.Empty;

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }

            float speed = GetEffectiveSpeed();

            if (speed <= 0f || !isActiveAndEnabled)
            {
                SkipToEnd();
                return;
            }

            playRoutine = StartCoroutine(PlayRoutine());
        }

        private float GetEffectiveSpeed()
        {
            return useGameSettings
                ? GameSettings.GetDialogueCharactersPerSecond()
                : charactersPerSecond;
        }

        /// <summary>
        /// Instantly shows the full text. Safe to call whether or not a
        /// reveal is currently in progress.
        /// </summary>
        public void SkipToEnd()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            bool wasPlaying = IsPlaying;

            if (label != null)
            {
                label.text = fullText;
            }

            IsPlaying = false;

            if (wasPlaying)
            {
                Completed?.Invoke();
            }
        }

        private IEnumerator PlayRoutine()
        {
            IsPlaying = true;

            if (label != null)
            {
                label.text = string.Empty;
            }

            float secondsPerChar = 1f / Mathf.Max(GetEffectiveSpeed(), 1f);

            for (int i = 0; i < fullText.Length; i++)
            {
                if (label != null)
                {
                    label.text = fullText[..(i + 1)];
                }

                char c = fullText[i];
                float wait = secondsPerChar;

                if (c is '.' or ',' or '!' or '?' or '\n')
                {
                    wait *= punctuationPauseMultiplier;
                }

                yield return new WaitForSeconds(wait);
            }

            playRoutine = null;
            IsPlaying = false;

            Completed?.Invoke();
        }

        private void OnDisable()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            IsPlaying = false;
        }
    }
}
