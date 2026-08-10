using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Cases;
using Verdict.Data.Characters;
using Verdict.Data.Narrative;
using Verdict.Runtime;
using Verdict.Systems;
using Verdict.Systems.Narrative;

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

        private CourtroomController courtroomController;

        private readonly List<ChoiceOptionView> spawnedOptions = new();

        /// <summary>
        /// Fired when the player chooses a courtroom action.
        ///
        /// The presenter does not execute the action itself.
        /// Another UI/gameplay layer can react to this.
        /// </summary>
        public event Action<PlayerAction> ActionRequested;

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

            if (dialoguePanelButton != null)
            {
                dialoguePanelButton.onClick.AddListener(
                    HandleContinuePressed);
            }

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

            if (dialoguePanelButton != null)
            {
                dialoguePanelButton.onClick.RemoveListener(
                    HandleContinuePressed);
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

            RefreshStatementAndActions();
        }

        private void HandleContinuePressed()
        {
            if (courtroomController == null)
            {
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

            if (!courtroomController.CanInteract)
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
                dialogueText.text =
                    entry.Line.Text ?? string.Empty;
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

        private void ShowStatement(
            StatementRuntime statement)
        {
            if (statementPanel != null)
            {
                statementPanel.SetActive(true);
            }

            if (statementText != null)
            {
                statementText.text = statement.Data.Text ?? string.Empty;
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

            IReadOnlyList<PlayerAction> actions =
                GetAvailableActions(statement);

            foreach (PlayerAction action in actions)
            {
                if (action == PlayerAction.None ||
                    action == PlayerAction.RemainSilent)
                {
                    continue;
                }

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

            ChoiceOptionView defaultOption =
                    Instantiate(
                        choiceOptionPrefab,
                        choiceOptionsContainer);

            defaultOption.SetText(
                    GetDisplayText(PlayerAction.RemainSilent));

            defaultOption.SetAction(
                PlayerAction.RemainSilent,
                HandleActionSelected);

            spawnedOptions.Add(defaultOption);
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
            HandleContinuePressed();
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

                    ActionRequested?.Invoke(action);
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

                case PlayerAction.CompareEvidence:
                    courtroomController.BeginCompareEvidence();

                    ActionRequested?.Invoke(action);
                    break;

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
