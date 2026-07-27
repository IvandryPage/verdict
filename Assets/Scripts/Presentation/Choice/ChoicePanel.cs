using System.Collections.Generic;
using UnityEngine;
using Verdict.Data.Narrative;
using Verdict.Presentation.Courtroom;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Presentation.Choice
{
    /// <summary>
    /// Shows up whenever the narrative graph reaches a Choice node, one
    /// button per option, and tells CourtroomController which one the
    /// player picked.
    /// </summary>
    public sealed class ChoicePanel : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("UI")]
        [SerializeField] private CanvasGroup root;
        [SerializeField] private RectTransform optionsContainer;
        [SerializeField] private ChoiceOptionView optionPrefab;

        private CourtroomController controller;
        private readonly List<ChoiceOptionView> spawned = new();

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
                Debug.LogError($"{nameof(ChoicePanel)}: Missing CourtroomBootstrap or Controller.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;
            controller.ChoiceRequested += HandleChoiceRequested;
            controller.CurrentStatementChanged += HandleCurrentStatementChanged;

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (controller == null)
            {
                return;
            }

            controller.ChoiceRequested -= HandleChoiceRequested;
            controller.CurrentStatementChanged -= HandleCurrentStatementChanged;
        }

        private void HandleChoiceRequested(ChoiceNodeData choice)
        {
            ClearOptions();

            if (choice == null || optionPrefab == null || optionsContainer == null)
            {
                return;
            }

            for (int i = 0; i < choice.Choices.Count; i++)
            {
                NarrativeChoiceOptionData option = choice.Choices[i];

                ChoiceOptionView view =
                    Instantiate(optionPrefab, optionsContainer);

                view.Bind(i, option.Text);

                int capturedIndex = i;
                view.Clicked += _ => HandleOptionClicked(capturedIndex);

                spawned.Add(view);
            }

            SetVisible(true);
        }

        private void HandleOptionClicked(int index)
        {
            controller.SelectChoice(index);
            SetVisible(false);
            ClearOptions();
        }

        private void HandleCurrentStatementChanged(StatementRuntime statement)
        {
            if (statement != null)
            {
                SetVisible(false);
            }
        }

        private void ClearOptions()
        {
            foreach (ChoiceOptionView view in spawned)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            spawned.Clear();
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
    }
}
