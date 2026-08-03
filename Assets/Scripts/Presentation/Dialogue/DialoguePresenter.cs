using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Cases;
using Verdict.Data.Narrative;
using Verdict.Presentation.Courtroom;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Presentation.Dialogue
{
    /// <summary>
    /// The dialogue box: portrait, speaker name, typewriter text,
    /// auto/manual continue (per-line WaitMode), and tap-to-skip typing.
    /// Camera/music/sound cues are handled separately by
    /// PresentationEventRouter - this class only shows text.
    /// </summary>
    public sealed class DialoguePresenter : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("UI")]
        [SerializeField] private CanvasGroup root;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TypewriterText typewriter;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject continuePrompt;

        [Header("Skip")]
        [Tooltip("If on, holding/toggling this lets the player fast-forward through PlayerInput lines too.")]
        [SerializeField] private Toggle skipModeToggle;

        private CourtroomController controller;
        private NarrativeDialogueEntryData currentEntry;
        private Coroutine autoAdvanceRoutine;

        private void Awake()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<CourtroomBootstrap>();
            }
        }

        private void Start()
        {
            if (bootstrap == null || bootstrap.Controller == null)
            {
                Debug.LogError($"{nameof(DialoguePresenter)}: Missing CourtroomBootstrap or Controller.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;
            controller.NarrativeEntryChanged += HandleNarrativeEntryChanged;
            controller.CurrentStatementChanged += HandleCurrentStatementChanged;
            controller.EndingTriggered += HandleEndingTriggered;
            controller.ChoiceRequested += HandleChoiceRequested;

            if (typewriter != null)
            {
                typewriter.Completed += HandleTypewriterCompleted;
            }

            RefreshFromController();
        }

        private void OnEnable()
        {
            continueButton?.onClick.AddListener(OnContinuePressed);
        }

        private void OnDisable()
        {
            continueButton?.onClick.RemoveListener(OnContinuePressed);
            StopAutoAdvance();
        }

        private void OnDestroy()
        {
            if (typewriter != null)
            {
                typewriter.Completed -= HandleTypewriterCompleted;
            }

            if (controller == null)
            {
                return;
            }

            controller.NarrativeEntryChanged -= HandleNarrativeEntryChanged;
            controller.CurrentStatementChanged -= HandleCurrentStatementChanged;
            controller.EndingTriggered -= HandleEndingTriggered;
            controller.ChoiceRequested -= HandleChoiceRequested;
        }

        /// <summary>
        /// Classic VN behaviour: first tap skips the typewriter to the
        /// end of the line; a second tap (or a tap once already fully
        /// shown) advances the narrative.
        /// </summary>
        public void OnContinuePressed()
        {
            if (controller == null)
            {
                return;
            }

            if (typewriter != null && typewriter.IsPlaying)
            {
                typewriter.SkipToEnd();
                return;
            }

            StopAutoAdvance();
            controller.ResumeNarrative();
        }

        private void HandleNarrativeEntryChanged(NarrativeDialogueEntryData entry)
        {
            currentEntry = entry;
            RefreshFromEntry(entry);
        }

        private void HandleChoiceRequested(ChoiceNodeData choice)
        {
            // The choice panel takes over from here.
            SetVisible(false);
        }

        private void HandleCurrentStatementChanged(StatementRuntime statement)
        {
            // dialogue can hide when gameplay starts
            if (statement != null)
            {
                SetVisible(false);
            }
        }

        private void HandleEndingTriggered(EndingData ending)
        {
            SetVisible(false);
        }

        private void HandleTypewriterCompleted()
        {
            if (continuePrompt != null)
            {
                continuePrompt.SetActive(true);
            }

            TryScheduleAutoAdvance();
        }

        private void RefreshFromController()
        {
            RefreshFromEntry(controller?.CurrentNarrativeEntry);
        }

        private void RefreshFromEntry(NarrativeDialogueEntryData entry)
        {
            StopAutoAdvance();

            if (entry == null || entry.Type != NarrativeDialogueEntryType.Line || entry.Line == null)
            {
                SetVisible(false);
                Clear();
                return;
            }

            SetVisible(true);

            if (speakerText != null)
            {
                speakerText.text = GetSpeakerLabel(entry.Line);
            }

            if (portraitImage != null)
            {
                Sprite portrait = GetPortrait(entry.Line);
                portraitImage.sprite = portrait;
                portraitImage.enabled = portrait != null;
            }

            if (continuePrompt != null)
            {
                continuePrompt.SetActive(false);
            }

            if (continueButton != null)
            {
                continueButton.interactable = true;
            }

            bool instant = entry.Line.WaitMode == NarrativeWaitMode.Instant;

            if (typewriter != null && !instant)
            {
                typewriter.Play(entry.Line.Text ?? string.Empty);
            }
            else if (typewriter != null)
            {
                typewriter.Play(entry.Line.Text ?? string.Empty);
                typewriter.SkipToEnd();
            }
        }

        /// <summary>
        /// WaitMode.Auto lines advance themselves after AutoAdvanceDelay
        /// once fully typed out. WaitMode.Instant lines behave the same
        /// but skip the typewriter (handled in RefreshFromEntry).
        /// WaitMode.PlayerInput waits for a tap, unless skip mode is on.
        /// </summary>
        private void TryScheduleAutoAdvance()
        {
            if (currentEntry?.Line == null)
            {
                return;
            }

            bool shouldAutoAdvance =
                currentEntry.Line.WaitMode == NarrativeWaitMode.Auto ||
                currentEntry.Line.WaitMode == NarrativeWaitMode.Instant ||
                (skipModeToggle != null && skipModeToggle.isOn);

            if (!shouldAutoAdvance)
            {
                return;
            }

            float delay = currentEntry.Line.WaitMode == NarrativeWaitMode.Auto
                ? Mathf.Max(0f, currentEntry.Line.AutoAdvanceDelay)
                : 0.15f; // small beat even in skip mode, so it doesn't feel instant/jarring

            autoAdvanceRoutine = StartCoroutine(AutoAdvanceRoutine(delay));
        }

        private IEnumerator AutoAdvanceRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            autoAdvanceRoutine = null;
            controller?.ResumeNarrative();
        }

        private void StopAutoAdvance()
        {
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }
        }

        private void Clear()
        {
            if (speakerText != null)
            {
                speakerText.text = string.Empty;
            }

            if (typewriter != null)
            {
                typewriter.Play(string.Empty);
            }

            if (portraitImage != null)
            {
                portraitImage.enabled = false;
            }

            if (continueButton != null)
            {
                continueButton.interactable = false;
            }
        }

        private void SetVisible(bool visible)
        {
            if (root == null)
            {
                return;
            }

            root.alpha = visible ? 1f : 0f;
            root.interactable = visible;
            root.blocksRaycasts = visible;
        }

        private static Sprite GetPortrait(NarrativeLineData line)
        {
            if (line?.Speaker == null)
            {
                return null;
            }

            foreach (var entry in line.Speaker.Portraits)
            {
                if (entry.Emotion == line.Emotion)
                {
                    return entry.Portrait;
                }
            }

            return line.Speaker.Portraits.Count > 0
                ? line.Speaker.Portraits[0].Portrait
                : null;
        }

        private static string GetSpeakerLabel(NarrativeLineData line)
        {
            if (line == null)
            {
                return string.Empty;
            }

            if (line.Speaker != null && !string.IsNullOrWhiteSpace(line.Speaker.DisplayName))
            {
                return line.Speaker.DisplayName;
            }

            return line.SpeakerType.ToString();
        }
    }
}
