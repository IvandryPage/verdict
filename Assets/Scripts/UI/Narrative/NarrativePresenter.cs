using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Cases;
using Verdict.Data.Characters;
using Verdict.Data.Evidence;
using Verdict.Data.Narrative;
using Verdict.Runtime;
using Verdict.Systems;
using Verdict.Systems.Narrative;
using Verdict.UI.Evidence;

namespace Verdict.UI.Narrative
{
    /// <summary>
    /// Presents narrative and courtroom interaction state.
    ///
    /// Responsibilities:
    /// - Show/hide dialogue UI.
    /// - Show/hide courtroom action choices.
    /// - Generate unique courtroom actions from the current statement's ArgumentRules.
    /// - Translate PlayerAction into predefined player-facing text.
    ///
    /// Does NOT:
    /// - Resolve arguments.
    /// - Modify CourtState.
    /// - Decide whether an argument succeeds.
    /// - Handle ChoiceNodeData narrative branching.
    /// </summary>
    public sealed class NarrativePresenter : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField]
        private GameObject dialoguePanel;

        [SerializeField]
        private TMP_Text dialogueText;

        [SerializeField]
        private TMP_Text speakerNameText;

        [SerializeField]
        private UnityEngine.UI.Image speakerPortrait;

        [SerializeField]
        private Button dialoguePanelButton;

        [Header("Dialogue Timing")]
        [SerializeField]
        private float wordRevealDelay = 0.025f;

        private Coroutine dialogueRevealCoroutine;
        private Coroutine statementRevealCoroutine;
        private string currentDialogueText = string.Empty;
        private string currentStatementText = string.Empty;
        private bool isDialogueRevealing;

        [Header("Courtroom Choice")]
        [SerializeField]
        private GameObject choicePanel;

        [SerializeField]
        private GameObject statementPanel;

        [SerializeField]
        private TMP_Text statementText;

        [SerializeField]
        private TMP_Text statementSpeaker;

        [SerializeField]
        private Transform choiceOptionsContainer;

        [SerializeField]
        private ChoiceOptionView choiceOptionPrefab;

        [Header("Evidence")]
        [SerializeField]
        private EvidencePresenter evidencePresenter;

        private CourtroomController courtroomController;

        private readonly List<ChoiceOptionView> spawnedOptions = new();

        /// <summary>
        /// Bind the presenter to the active CourtroomController.
        /// </summary>
        public void Bind(CourtroomController controller)
        {
            Unbind();

            courtroomController = controller;

            if (courtroomController == null)
            {
                return;
            }

            courtroomController.NarrativeEntryChanged +=
                HandleNarrativeEntryChanged;

            courtroomController.CurrentStatementChanged +=
                HandleCurrentStatementChanged;

            courtroomController.ArgumentResolved +=
                HandleArgumentResolved;

            if (dialoguePanelButton != null)
            {
                dialoguePanelButton.onClick.AddListener(
                    HandleContinuePressed);
            }

            if (evidencePresenter != null)
            {
                evidencePresenter.CancelRequested +=
                    HandleEvidenceCancelled;

                evidencePresenter.PresentRequested +=
                    HandleEvidencePresented;
            }

            courtroomController.CourtroomStateChanged +=
                HandleCourtroomStateChanged;

            courtroomController.ChoiceRequested +=
                HandleChoiceRequested;

            Refresh();
        }

        /// <summary>
        /// Remove event subscriptions.
        /// </summary>
        public void Unbind()
        {
            if (courtroomController == null)
            {
                return;
            }

            courtroomController.NarrativeEntryChanged -=
                HandleNarrativeEntryChanged;

            courtroomController.CurrentStatementChanged -=
                HandleCurrentStatementChanged;

            courtroomController.ArgumentResolved -=
                HandleArgumentResolved;

            courtroomController.CourtroomStateChanged -=
                HandleCourtroomStateChanged;

            courtroomController.ChoiceRequested -=
                HandleChoiceRequested;

            if (dialoguePanelButton != null)
            {
                dialoguePanelButton.onClick.RemoveListener(
                    HandleContinuePressed);
            }

            if (evidencePresenter != null)
            {
                evidencePresenter.CancelRequested -=
                    HandleEvidenceCancelled;

                evidencePresenter.PresentRequested -=
                    HandleEvidencePresented;
            }

            courtroomController = null;
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// Refresh the entire presentation from the current
        /// CourtroomController state.
        /// </summary>
        public void Refresh()
        {
            if (courtroomController == null)
            {
                HideAll();
                return;
            }

            if (courtroomController.IsWaitingForChoice)
            {
                BuildNarrativeChoiceOptions(
                    courtroomController.CurrentChoice);

                ShowChoicePanel();
                return;
            }

            RefreshStatementAndActions();
        }

        private void HandleContinuePressed()
        {
            if (courtroomController == null)
            {
                return;
            }

            if (isDialogueRevealing)
            {
                CompleteDialogueReveal();
                return;
            }

            NarrativeCoordinator narrative = courtroomController.Narrative;
            if (narrative != null &&
                (narrative.IsPlaying || narrative.IsWaitingForStatement || narrative.IsWaitingForChoice))
            {
                courtroomController.ResumeNarrative();
                return;
            }

            if (courtroomController.CanInteract)
            {
                return;
            }

            courtroomController.ResumeNarrative();
        }

        private void HandleNarrativeEntryChanged(
            NarrativeDialogueEntryData entry)
        {
            ShowDialogue(entry);

            HideChoicePanel();
        }

        private void HandleCurrentStatementChanged(
            StatementRuntime statement)
        {
            RefreshStatementAndActions();
        }

        private void HandleChoiceRequested(
            ChoiceNodeData choice)
        {
            if (choice == null)
            {
                return;
            }

            ClearActionOptions();

            if (statementPanel != null)
            {
                statementPanel.SetActive(false);
            }

            HideDialoguePanel();

            BuildNarrativeChoiceOptions(choice);
            ShowChoicePanel();
        }

        private void HandleCourtroomStateChanged(
            CourtroomState state)
        {
            if (state == CourtroomState.EvidenceSelection)
            {
                OpenEvidencePanel();
            }
        }

        private void RefreshStatementAndActions()
        {
            StatementRuntime statement =
                courtroomController.CurrentStatement;

            if (statement == null)
            {
                HideChoicePanel();
                return;
            }

            ShowStatement(statement);

            bool canUseActionPanel =
                courtroomController.CanInteract ||
                (courtroomController.CourtroomState == CourtroomState.Statement && statement != null);

            if (!canUseActionPanel)
            {
                HideChoicePanel();
                return;
            }

            BuildActionOptions(statement);

            ShowChoicePanel();
        }

        private void ShowDialogue(
            NarrativeDialogueEntryData entry)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            if (entry?.Line == null)
            {
                ClearDialogueVisuals();
                return;
            }

            if (dialogueText != null)
            {
                StartDialogueReveal(entry.Line.Text ?? string.Empty);
            }

            bool isCharacter =
                entry.Line.SpeakerType ==
                NarrativeSpeakerType.Character;

            if (!isCharacter)
            {
                ShowNarrator();
                return;
            }

            CharacterData character =
                courtroomController.GetCharacter(
                    entry.Line.Speaker.Id);

            if (character == null)
            {
                Debug.LogWarning(
                    $"NarrativePresenter: " +
                    $"Character '{entry.Line.Speaker}' was not found.");

                if (speakerNameText != null)
                {
                    speakerNameText.text =
                        entry.Line.Speaker.DisplayName ?? string.Empty;
                }

                if (speakerPortrait != null)
                {
                    speakerPortrait.enabled = false;
                    speakerPortrait.sprite = null;
                }

                return;
            }

            if (speakerNameText != null)
            {
                speakerNameText.text =
                    character.DisplayName;
            }

            Sprite portrait =
                CharacterPortraitResolver.Resolve(
                    character,
                    entry.Line.Emotion);

            if (speakerPortrait != null)
            {
                speakerPortrait.sprite = portrait;
                speakerPortrait.enabled = portrait != null;
            }

        }

        private void ShowNarrator()
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = "Narator";
            }

            if (speakerPortrait != null)
            {
                speakerPortrait.enabled = false;
            }
        }

        private void ClearDialogueVisuals()
        {
            StopDialogueReveal();
            StopStatementReveal();

            if (dialogueText != null)
            {
                dialogueText.text = string.Empty;
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = string.Empty;
            }

            if (speakerPortrait != null)
            {
                speakerPortrait.enabled = false;
                speakerPortrait.sprite = null;
            }
        }

        private void StartStatementReveal(string text)
        {
            StopStatementReveal();

            currentStatementText = text;
            statementText.text = string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            statementRevealCoroutine = StartCoroutine(
                RevealStatementText(text));
        }

        private void CompleteStatementReveal()
        {
            StopStatementReveal();

            if (statementText != null)
            {
                statementText.text = currentStatementText;
            }
        }

        private void StopStatementReveal()
        {
            if (statementRevealCoroutine != null)
            {
                StopCoroutine(statementRevealCoroutine);
                statementRevealCoroutine = null;
            }
        }

        private IEnumerator RevealStatementText(string text)
        {
            statementRevealCoroutine = null;
            statementText.text = string.Empty;

            string[] words = text.Split(
                new[] { ' ' },
                StringSplitOptions.None);

            for (int index = 0; index < words.Length; index++)
            {
                statementText.text += words[index];

                if (index < words.Length - 1)
                {
                    statementText.text += " ";
                }

                yield return new WaitForSeconds(wordRevealDelay);
            }

            statementRevealCoroutine = null;
        }

        private void StartDialogueReveal(string text)
        {
            StopDialogueReveal();

            currentDialogueText = text;
            dialogueText.text = string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                isDialogueRevealing = false;
                return;
            }

            dialogueRevealCoroutine = StartCoroutine(
                RevealDialogueText(text));
        }

        private void CompleteDialogueReveal()
        {
            StopDialogueReveal();

            if (dialogueText != null)
            {
                dialogueText.text = currentDialogueText;
            }

            isDialogueRevealing = false;
        }

        private void StopDialogueReveal()
        {
            if (dialogueRevealCoroutine != null)
            {
                StopCoroutine(dialogueRevealCoroutine);
                dialogueRevealCoroutine = null;
            }

            isDialogueRevealing = false;
        }

        private IEnumerator RevealDialogueText(string text)
        {
            isDialogueRevealing = true;
            dialogueText.text = string.Empty;

            string[] words = text.Split(
                new[] { ' ' },
                StringSplitOptions.None);

            for (int index = 0; index < words.Length; index++)
            {
                dialogueText.text += words[index];

                if (index < words.Length - 1)
                {
                    dialogueText.text += " ";
                }

                yield return new WaitForSeconds(wordRevealDelay);
            }

            isDialogueRevealing = false;
            dialogueRevealCoroutine = null;
        }

        private void ShowStatement(
            StatementRuntime statement)
        {
            if (statementPanel != null)
            {
                statementPanel.SetActive(true);
            }

            if (statementText != null)
            {
                StartStatementReveal(statement.Data.Text ?? string.Empty);
            }

            if (statementSpeaker != null)
            {
                CharacterData character =
                    courtroomController.CurrentWitness?.Character?.Data;

                statementSpeaker.text =
                    character?.DisplayName ?? string.Empty;
            }
        }

        private void BuildActionOptions(
            StatementRuntime statement)
        {
            ClearActionOptions();

            List<PlayerAction> actions =
                GetAvailableActions(statement)
                    .Where(action => action != PlayerAction.None)
                    .ToList();

            if (!actions.Contains(PlayerAction.RemainSilent))
            {
                actions.Add(PlayerAction.RemainSilent);
            }

            List<PlayerAction> randomizedActions =
                actions.OrderBy(_ => UnityEngine.Random.value)
                       .ToList();

            foreach (PlayerAction action in randomizedActions)
            {
                ChoiceOptionView option =
                    Instantiate(
                        choiceOptionPrefab,
                        choiceOptionsContainer);

                option.SetText(
                    GetDisplayText(action));

                option.SetAction(
                    action,
                    HandleActionSelected);

                spawnedOptions.Add(option);
            }
        }

        private void BuildNarrativeChoiceOptions(
            ChoiceNodeData choice)
        {
            ClearActionOptions();

            if (choice == null || choice.Choices == null ||
                choice.Choices.Count == 0)
            {
                return;
            }

            List<int> choiceOrder =
                Enumerable.Range(0, choice.Choices.Count)
                          .OrderBy(_ => UnityEngine.Random.value)
                          .ToList();

            foreach (int choiceIndex in choiceOrder)
            {
                NarrativeChoiceOptionData choiceOption =
                    choice.Choices[choiceIndex];

                ChoiceOptionView option =
                    Instantiate(
                        choiceOptionPrefab,
                        choiceOptionsContainer);

                option.SetChoice(
                    choiceOption?.Text ?? string.Empty,
                    choiceIndex,
                    HandleNarrativeChoiceSelected);

                spawnedOptions.Add(option);
            }
        }

        private void HandleNarrativeChoiceSelected(
            int choiceIndex)
        {
            if (courtroomController == null)
            {
                return;
            }

            HideChoicePanel();

            if (!courtroomController.SelectChoice(choiceIndex))
            {
                Debug.LogWarning(
                    $"NarrativePresenter: Invalid choice index {choiceIndex}.");
            }
        }

        /// <summary>
        /// Gets all unique PlayerActions available on the current
        /// statement.
        ///
        /// Source of truth:
        /// Statement -> Claims -> ArgumentRules -> Action.
        /// </summary>
        private static IReadOnlyList<PlayerAction> GetAvailableActions(
            StatementRuntime statement)
        {
            if (statement == null)
            {
                return Array.Empty<PlayerAction>();
            }

            return statement.Claims
                .SelectMany(claim =>
                    claim.Data.ArgumentRules)
                .Select(rule =>
                    rule.Action)
                .Where(action =>
                    action != PlayerAction.None)
                .Distinct()
                .ToList();
        }

        private void HandleActionSelected(
            PlayerAction action)
        {
            if (courtroomController == null)
            {
                return;
            }

            if (!courtroomController.CanInteract)
            {
                return;
            }

            ExecuteAction(action);
        }

        private void HandleArgumentResolved(
            ResolverResult result)
        {
            HandleContinuePressed();
        }

        private void HandleEvidencePresented(
            IReadOnlyList<EvidenceData> evidence)
        {
            if (courtroomController == null)
            {
                return;
            }

            ResolverResult result =
                courtroomController.ResolvePresentEvidence(
                    evidence);

            HandleContinuePressed();
        }

        private void HandleEvidenceCancelled()
        {
            if (courtroomController == null)
            {
                return;
            }

            courtroomController.CancelEvidenceSelection();

            Refresh();
        }

        private void ExecuteAction(
            PlayerAction action)
        {
            HideChoicePanel();

            switch (action)
            {
                case PlayerAction.Press:
                    courtroomController.BeginPress();
                    courtroomController.ResolvePress();
                    break;

                case PlayerAction.Question:
                    courtroomController.BeginQuestion();
                    courtroomController.ResolveQuestion();
                    break;

                case PlayerAction.RemainSilent:
                    courtroomController.RemainSilent();
                    break;

                case PlayerAction.PresentEvidence:
                    courtroomController.BeginPresentEvidence();
                    OpenEvidencePanel();
                    break;

                case PlayerAction.Bluff:
                    courtroomController.BeginBluff();
                    courtroomController.ResolveBluff();
                    break;

                case PlayerAction.Threaten:
                    courtroomController.BeginThreaten();
                    courtroomController.ResolveThreaten();
                    break;

                case PlayerAction.Object:
                    courtroomController.BeginObject();
                    courtroomController.ResolveObject();
                    break;

                case PlayerAction.Interrupt:
                    courtroomController.BeginInterrupt();
                    courtroomController.ResolveInterrupt();
                    break;

                // case PlayerAction.CompareEvidence:
                //     courtroomController.BeginCompareEvidence();

                //     ActionRequested?.Invoke(action);
                //     break;

                default:
                    Debug.LogWarning(
                        $"Unhandled PlayerAction: {action}");
                    break;
            }
        }

        private static string GetDisplayText(
            PlayerAction action)
        {
            return action switch
            {
                PlayerAction.PresentEvidence =>
                    "Sepertinya saya perlu memaparkan bukti ini.",

                PlayerAction.Press =>
                    "Saya perlu menekan kesaksiannya lebih lanjut.",

                PlayerAction.Question =>
                    "Saya perlu menanyakan ulang terkait hal ini.",

                PlayerAction.RemainSilent =>
                    "Untuk saat ini, lebih baik saya diam.",

                PlayerAction.Bluff =>
                    "Mungkin saya bisa menggertak dan melihat reaksinya.",

                PlayerAction.Threaten =>
                    "Saya perlu memberikan sedikit tekanan.",

                PlayerAction.Object =>
                    "Saya rasa saya harus mengajukan keberatan.",

                PlayerAction.Interrupt =>
                    "Saya harus menyela sekarang.",

                PlayerAction.CompareEvidence =>
                    "Saya perlu membandingkan bukti yang ada.",

                _ =>
                    string.Empty
            };
        }

        private void ShowChoicePanel()
        {
            HideDialoguePanel();

            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
            }
        }

        private void HideChoicePanel()
        {
            ClearActionOptions();

            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
        }

        private void HideDialoguePanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private void ClearActionOptions()
        {
            foreach (ChoiceOptionView option in spawnedOptions)
            {
                if (option != null)
                {
                    Destroy(option.gameObject);
                }
            }

            spawnedOptions.Clear();
        }

        private void OpenEvidencePanel()
        {
            if (evidencePresenter == null)
            {
                Debug.LogError(
                    "EvidencePresenter is not assigned.");

                return;
            }

            IReadOnlyList<EvidenceData> availableEvidence =
                courtroomController.GetAvailableEvidence();

            evidencePresenter.Open(
                availableEvidence);
        }

        private void HideAll()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }

            if (statementPanel != null)
            {
                statementPanel.SetActive(false);
            }

            ClearActionOptions();
        }
    }
}
