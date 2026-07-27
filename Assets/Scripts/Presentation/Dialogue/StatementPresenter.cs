using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Verdict.Data.Cases;
using Verdict.Presentation.Courtroom;
using Verdict.Presentation.Evidence;
using Verdict.Runtime;
using Verdict.Systems;

namespace Verdict.Presentation.Statement
{
    /// <summary>
    /// Displays the current statement and forwards player actions
    /// to CourtroomController.
    /// </summary>
    public sealed class StatementPresenter : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private CourtroomBootstrap bootstrap;

        [Header("Root")]
        [SerializeField] private CanvasGroup root;

        [Header("UI")]
        [SerializeField] private TMP_Text witnessNameText;
        [SerializeField] private TMP_Text statementText;

        [Header("Actions")]
        [SerializeField] private Button pressButton;
        [SerializeField] private Button questionButton;
        [SerializeField] private Button presentEvidenceButton;
        [SerializeField] private Button remainSilentButton;

        [Header("Evidence")]
        [SerializeField] private EvidencePanel evidencePanel;

        private CourtroomController controller;
        private StatementRuntime currentStatement;

        private void Awake()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<CourtroomBootstrap>();
            }

            if (bootstrap == null || bootstrap.Controller == null)
            {
                Debug.LogError($"{nameof(StatementPresenter)}: Missing CourtroomBootstrap.");
                enabled = false;
                return;
            }

            controller = bootstrap.Controller;

            controller.CurrentStatementChanged += HandleStatementChanged;
            controller.ArgumentResolved += HandleArgumentResolved;
        }

        private void Start()
        {
            Refresh();
        }

        private void OnEnable()
        {
            pressButton?.onClick.AddListener(OnPressClicked);
            questionButton?.onClick.AddListener(OnQuestionClicked);
            remainSilentButton?.onClick.AddListener(OnRemainSilentClicked);
            presentEvidenceButton?.onClick.AddListener(OnPresentEvidenceClicked);
        }

        private void OnDisable()
        {
            pressButton?.onClick.RemoveListener(OnPressClicked);
            questionButton?.onClick.RemoveListener(OnQuestionClicked);
            remainSilentButton?.onClick.RemoveListener(OnRemainSilentClicked);
            presentEvidenceButton?.onClick.RemoveListener(OnPresentEvidenceClicked);
        }

        private void OnDestroy()
        {
            if (controller == null)
            {
                return;
            }

            controller.CurrentStatementChanged -= HandleStatementChanged;
            controller.ArgumentResolved -= HandleArgumentResolved;
        }

        private void HandleStatementChanged(StatementRuntime statement)
        {
            currentStatement = statement;
            Refresh();
        }

        private void HandleArgumentResolved(ResolverResult result)
        {
            SetVisible(false);
        }

        private void Refresh()
        {
            currentStatement = controller?.CurrentStatement;

            if (currentStatement == null || !controller.CanInteract)
            {
                Clear();
                SetVisible(false);
                return;
            }

            SetVisible(true);

            if (statementText != null)
            {
                statementText.text = currentStatement.Data.Text;
            }

            if (witnessNameText != null)
            {
                witnessNameText.text =
                    controller.CurrentWitness?.Data?.Character?.DisplayName ??
                    controller.CurrentWitness?.Data?.Id ??
                    string.Empty;
            }

            bool interactable = controller.CanInteract;

            pressButton.interactable = interactable;
            questionButton.interactable = interactable;
            remainSilentButton.interactable = interactable;
            presentEvidenceButton.interactable = interactable;
        }

        private void Clear()
        {
            if (statementText != null)
            {
                statementText.text = string.Empty;
            }

            if (witnessNameText != null)
            {
                witnessNameText.text = string.Empty;
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

        private void OnPressClicked()
        {
            controller.Press();
        }

        private void OnQuestionClicked()
        {
            controller.Question();
        }

        private void OnRemainSilentClicked()
        {
            controller.RemainSilent();
        }

        private void OnPresentEvidenceClicked()
        {
            if (evidencePanel == null)
            {
                Debug.LogWarning($"{nameof(StatementPresenter)}: No EvidencePanel assigned.");
                return;
            }

            evidencePanel.Open();
        }
    }
}
