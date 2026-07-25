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
    public sealed class DialoguePresenter : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("UI")]
        [SerializeField] private CanvasGroup root;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button continueButton;

        private CourtroomController controller;
        private NarrativeDialogueEntryData currentEntry;

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
            controller.PresentationEventTriggered += HandlePresentationEvent;
            controller.GameplayEventTriggered += HandleGameplayEvent;
            controller.CurrentStatementChanged += HandleCurrentStatementChanged;
            controller.EndingTriggered += HandleEndingTriggered;

            RefreshFromController();
        }

        private void OnEnable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinuePressed);
            }
        }

        private void OnDisable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinuePressed);
            }
        }

        private void OnDestroy()
        {
            if (controller == null)
            {
                return;
            }

            controller.NarrativeEntryChanged -= HandleNarrativeEntryChanged;
            controller.PresentationEventTriggered -= HandlePresentationEvent;
            controller.GameplayEventTriggered -= HandleGameplayEvent;
            controller.CurrentStatementChanged -= HandleCurrentStatementChanged;
            controller.EndingTriggered -= HandleEndingTriggered;
        }

        public void OnContinuePressed()
        {
            if (controller == null)
            {
                return;
            }

            controller.ResumeNarrative();
        }

        private void HandleNarrativeEntryChanged(NarrativeDialogueEntryData entry)
        {
            currentEntry = entry;
            RefreshFromEntry(entry);
        }

        private void HandlePresentationEvent(NarrativeEventData eventData)
        {
            // hook for camera/audio/VFX later
        }

        private void HandleGameplayEvent(GameplayNodeData node)
        {
            // hook for gameplay transitions later
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

        private void RefreshFromController()
        {
            RefreshFromEntry(controller?.CurrentNarrativeEntry);
        }

        private void RefreshFromEntry(NarrativeDialogueEntryData entry)
        {
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

            if (dialogueText != null)
            {
                dialogueText.text = entry.Line.Text ?? string.Empty;
            }

            if (portraitImage != null)
            {
                portraitImage.enabled = entry.Line.Speaker != null;
            }

            if (continueButton != null)
            {
                continueButton.interactable = true;
            }
        }

        private void Clear()
        {
            if (speakerText != null)
            {
                speakerText.text = string.Empty;
            }

            if (dialogueText != null)
            {
                dialogueText.text = string.Empty;
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

        private static string GetSpeakerLabel(NarrativeLineData line)
        {
            if (line == null)
            {
                return string.Empty;
            }

            if (line.Speaker != null && !string.IsNullOrWhiteSpace(line.Speaker.name))
            {
                return line.Speaker.name;
            }

            return line.SpeakerType.ToString();
        }
    }
}
